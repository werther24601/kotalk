#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  ./scripts/release/release-publish-github.sh --version 2026.04.16-alpha.4 [options]

Options:
  --remote <name>          Git remote name. Default: github-public
  --repo <owner/name>      GitHub repository in owner/name form
  --token <token>          GitHub token
  --target-commitish <ref> Branch or commit to associate when creating the tag
  --notes <path>           Release notes markdown file
  --dry-run                Print planned uploads without calling the API
EOF
}

version=""
remote_name="github-public"
repo_full_name=""
token="${GITHUB_RELEASE_TOKEN:-${GITHUB_TOKEN:-}}"
target_commitish=""
notes_path=""
dry_run="false"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      version="${2:-}"
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
if [[ -z "$repo_full_name" ]]; then
  if [[ "$remote_url" =~ ^https?://github\.com/(.+?)(\.git)?$ ]]; then
    repo_full_name="${BASH_REMATCH[1]}"
  elif [[ "$remote_url" =~ ^git@github\.com:(.+)\.git$ ]]; then
    repo_full_name="${BASH_REMATCH[1]}"
  fi
fi

if [[ -z "$repo_full_name" ]]; then
  echo "Unable to infer GitHub repository name from remote: $remote_name" >&2
  exit 1
fi

if [[ -z "$token" && -f "$repo_root/.workspace-secrets/github-public.pat" ]]; then
  token="$(tr -d '\r\n' < "$repo_root/.workspace-secrets/github-public.pat")"
fi

if [[ "$dry_run" != "true" && -z "$token" ]]; then
  echo "A GitHub token is required. Use --token, GITHUB_RELEASE_TOKEN, or GITHUB_TOKEN." >&2
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

echo "GitHub release target: https://github.com/$repo_full_name"
echo "Remote: $remote_name"
echo "Version: $version"
echo "Target commitish: $target_commitish"
printf 'Assets:\n'
printf '  - %s\n' "${asset_files[@]#$release_root/}"

if [[ "$dry_run" == "true" ]]; then
  exit 0
fi

api_root="https://api.github.com/repos/${repo_full_name}"
release_api="${api_root}/releases"
tag_api="${release_api}/tags/${version}"
auth_header="Authorization: Bearer $token"
accept_header="Accept: application/vnd.github+json"
api_version_header="X-GitHub-Api-Version: 2022-11-28"
tmp_response="$(mktemp)"
trap 'rm -f "$tmp_response"' EXIT

existing_status="$(curl -sS -o "$tmp_response" -w '%{http_code}' \
  -H "$auth_header" \
  -H "$accept_header" \
  -H "$api_version_header" \
  "$tag_api")"

if [[ "$existing_status" == "200" ]]; then
  existing_release_id="$(python3 - <<'PY' "$tmp_response"
import json, sys
with open(sys.argv[1], "r", encoding="utf-8") as fh:
    print(json.load(fh)["id"])
PY
)"
  curl -sS -X DELETE \
    -H "$auth_header" \
    -H "$accept_header" \
    -H "$api_version_header" \
    "${release_api}/${existing_release_id}" >/dev/null
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
    "prerelease": prerelease,
    "make_latest": "false" if prerelease else "true",
}, ensure_ascii=False))
PY
)"

create_status="$(curl -sS -o "$tmp_response" -w '%{http_code}' \
  -X POST \
  -H "$auth_header" \
  -H "$accept_header" \
  -H "$api_version_header" \
  -H 'Content-Type: application/json' \
  -d "$create_payload" \
  "$release_api")"

if [[ "$create_status" != "201" ]]; then
  echo "Failed to create GitHub release. HTTP $create_status" >&2
  cat "$tmp_response" >&2
  exit 1
fi

upload_url="$(python3 - <<'PY' "$tmp_response"
import json, sys
with open(sys.argv[1], "r", encoding="utf-8") as fh:
    print(json.load(fh)["upload_url"].split("{", 1)[0])
PY
)"

for asset in "${asset_files[@]}"; do
  name="$(basename "$asset")"
  curl -sS \
    -X POST \
    -H "$auth_header" \
    -H "$accept_header" \
    -H "$api_version_header" \
    -H 'Content-Type: application/octet-stream' \
    --data-binary @"$asset" \
    "${upload_url}?name=${name}" >/dev/null
done

echo "Published GitHub release $version with ${#asset_files[@]} assets."
