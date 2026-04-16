#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  ./scripts/release/release-publish-forge.sh --version v0.2.0-alpha.1 [options]

Options:
  --remote <name>          Git remote name. Default: public-stage
  --base-url <url>         Forge base URL. Example: https://forge.example.com
  --repo <owner/name>      Repository in owner/name form
  --token <token>          Gitea API token
  --target-commitish <ref> Branch or commit to associate when creating the tag
  --notes <path>           Release notes markdown file
  --dry-run                Print planned uploads without calling the API
EOF
}

version=""
remote_name="public-stage"
base_url=""
repo_full_name=""
token="${FORGE_RELEASE_TOKEN:-${GITEA_RELEASE_TOKEN:-}}"
target_commitish=""
notes_path=""
dry_run="false"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      version="${2:-}"
      shift 2
      ;;
    --base-url)
      base_url="${2:-}"
      shift 2
      ;;
    --remote)
      remote_name="${2:-}"
      shift 2
      ;;
    --repo)
      repo_full_name="${2:-}"
      shift 2
      ;;
    --token)
      token="${2:-}"
      shift 2
      ;;
    --target-commitish)
      target_commitish="${2:-}"
      shift 2
      ;;
    --notes)
      notes_path="${2:-}"
      shift 2
      ;;
    --dry-run)
      dry_run="true"
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage >&2
      exit 1
      ;;
  esac
done

if [[ -z "$version" ]]; then
  usage >&2
  exit 1
fi

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
release_root="$repo_root/release-assets/releases/$version"

if [[ ! -d "$release_root" ]]; then
  echo "Release bundle not found: $release_root" >&2
  exit 1
fi

remote_url="$(git -C "$repo_root" remote get-url "$remote_name" 2>/dev/null || true)"

if [[ -z "$base_url" ]]; then
  if [[ "$remote_url" =~ ^(https?://[^/]+)(/open-source/projects)/([^/]+)/([^/]+?)(\.git)?$ ]]; then
    base_url="${BASH_REMATCH[1]}${BASH_REMATCH[2]}"
  elif [[ "$remote_url" =~ ^(https?://[^/]+)/(.+?)(\.git)?$ ]]; then
    base_url="${BASH_REMATCH[1]}"
  elif [[ "$remote_url" =~ ^git@([^:]+):(.+)\.git$ ]]; then
    base_url="https://${BASH_REMATCH[1]}"
  fi
fi

if [[ -z "$repo_full_name" ]]; then
  if [[ "$remote_url" =~ ^https?://[^/]+/open-source/projects/([^/]+)/([^/]+?)(\.git)?$ ]]; then
    repo_full_name="${BASH_REMATCH[1]}/${BASH_REMATCH[2]}"
  elif [[ "$remote_url" =~ ^https?://[^/]+/(.+?)(\.git)?$ ]]; then
    repo_full_name="${BASH_REMATCH[1]}"
  elif [[ "$remote_url" =~ ^git@[^:]+:(.+)\.git$ ]]; then
    repo_full_name="${BASH_REMATCH[1]}"
  fi
fi

if [[ -z "$base_url" || -z "$repo_full_name" ]]; then
  echo "Unable to infer forge base URL or repository name from origin remote." >&2
  exit 1
fi

if [[ -z "$notes_path" ]]; then
  notes_path="$release_root/RELEASE_NOTES.ko.md"
fi

if [[ -n "$notes_path" && ! -f "$notes_path" ]]; then
  echo "Release notes file not found: $notes_path" >&2
  exit 1
fi

if [[ -z "$target_commitish" ]]; then
  target_commitish="main"
fi

basic_auth_user=""
basic_auth_password=""

if [[ -z "$token" ]]; then
  for fallback_path in \
    "$repo_root/.workspace-secrets/${remote_name}.token" \
    "$repo_root/.workspace-secrets/forge-release.token"; do
    if [[ -f "$fallback_path" ]]; then
      token="$(tr -d '\r\n' < "$fallback_path")"
      break
    fi
  done
fi

if [[ -z "$token" && -n "$remote_url" ]]; then
  credential_output="$(printf 'url=%s\n\n' "$remote_url" | git credential fill 2>/dev/null || true)"
  if [[ -n "$credential_output" ]]; then
    while IFS='=' read -r key value; do
      case "$key" in
        username) basic_auth_user="$value" ;;
        password) basic_auth_password="$value" ;;
      esac
    done <<< "$credential_output"
  fi
fi

if [[ "$dry_run" != "true" && -z "$token" && ( -z "$basic_auth_user" || -z "$basic_auth_password" ) ]]; then
  echo "A Gitea API token or basic credential is required. Use --token, FORGE_RELEASE_TOKEN, a local secret file, or configured git credentials." >&2
  exit 1
fi

api_root="${base_url%/}/api/v1/repos/${repo_full_name}"
release_api="${api_root}/releases"
tag_api="${release_api}/tags/${version}"
pre_release="false"

case "$version" in
  *alpha*|*beta*|*rc*)
    pre_release="true"
    ;;
esac

mapfile -t asset_files < <(
  {
    find "$release_root" -type f \( -name '*.zip' -o -name '*.exe' -o -name '*.apk' \)
    [[ -f "$release_root/SHA256SUMS.txt" ]] && printf '%s\n' "$release_root/SHA256SUMS.txt"
    [[ -f "$release_root/version.json" ]] && printf '%s\n' "$release_root/version.json"
  } | sort
)

if [[ ${#asset_files[@]} -eq 0 ]]; then
  echo "No release assets found in $release_root" >&2
  exit 1
fi

echo "Forge release target: $base_url/$repo_full_name"
echo "Remote: $remote_name"
echo "Version: $version"
echo "Target commitish: $target_commitish"
printf 'Assets:\n'
printf '  - %s\n' "${asset_files[@]#$release_root/}"

if [[ "$dry_run" == "true" ]]; then
  exit 0
fi

auth_args=()
if [[ -n "$token" ]]; then
  auth_args=(-H "Authorization: token $token")
else
  auth_args=(-u "$basic_auth_user:$basic_auth_password")
fi
tmp_response="$(mktemp)"
trap 'rm -f "$tmp_response"' EXIT

existing_status="$(curl -sS -o "$tmp_response" -w '%{http_code}' "${auth_args[@]}" "$tag_api")"
if [[ "$existing_status" == "200" ]]; then
  existing_release_id="$(python3 - <<'PY' "$tmp_response"
import json, sys
with open(sys.argv[1], "r", encoding="utf-8") as fh:
    print(json.load(fh)["id"])
PY
)"
  curl -sS -X DELETE "${auth_args[@]}" "${release_api}/${existing_release_id}" >/dev/null
fi

release_body="$(cat "$notes_path")"

create_payload="$(python3 - <<'PY' "$version" "$release_body" "$pre_release" "$target_commitish"
import json, sys
version, body, prerelease, target_commitish = sys.argv[1], sys.argv[2], sys.argv[3] == "true", sys.argv[4]
print(json.dumps({
    "tag_name": version,
    "target_commitish": target_commitish,
    "name": version,
    "body": body,
    "draft": False,
    "prerelease": prerelease
}, ensure_ascii=False))
PY
)"

create_status="$(curl -sS -o "$tmp_response" -w '%{http_code}' \
  -X POST \
  "${auth_args[@]}" \
  -H 'Content-Type: application/json' \
  -d "$create_payload" \
  "$release_api")"

if [[ "$create_status" != "201" ]]; then
  echo "Failed to create forge release. HTTP $create_status" >&2
  cat "$tmp_response" >&2
  exit 1
fi

release_id="$(python3 - <<'PY' "$tmp_response"
import json, sys
with open(sys.argv[1], "r", encoding="utf-8") as fh:
    print(json.load(fh)["id"])
PY
)"

for asset in "${asset_files[@]}"; do
  name="$(basename "$asset")"
  curl -sS \
    -X POST \
    "${auth_args[@]}" \
    -H 'Content-Type: application/octet-stream' \
    --data-binary @"$asset" \
    "${release_api}/${release_id}/assets?name=${name}" >/dev/null
done

echo "Published forge release $version with ${#asset_files[@]} assets."
