#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  ./scripts/release/release-publish-forge.sh --version v0.2.0-alpha.1 [options]

Options:
  --base-url <url>         Forge base URL. Example: https://forge.example.com
  --repo <owner/name>      Repository in owner/name form
  --token <token>          Gitea API token
  --dry-run                Print planned uploads without calling the API
EOF
}

version=""
base_url=""
repo_full_name=""
token="${FORGE_RELEASE_TOKEN:-${GITEA_RELEASE_TOKEN:-}}"
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
    --repo)
      repo_full_name="${2:-}"
      shift 2
      ;;
    --token)
      token="${2:-}"
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

origin_url="$(git -C "$repo_root" remote get-url origin 2>/dev/null || true)"

if [[ -z "$base_url" ]]; then
  if [[ "$origin_url" =~ ^(https?://[^/]+)/(.+)\.git$ ]]; then
    base_url="${BASH_REMATCH[1]}"
  elif [[ "$origin_url" =~ ^git@([^:]+):(.+)\.git$ ]]; then
    base_url="https://${BASH_REMATCH[1]}"
  fi
fi

if [[ -z "$repo_full_name" ]]; then
  if [[ "$origin_url" =~ ^https?://[^/]+/(.+)\.git$ ]]; then
    repo_full_name="${BASH_REMATCH[1]}"
  elif [[ "$origin_url" =~ ^git@[^:]+:(.+)\.git$ ]]; then
    repo_full_name="${BASH_REMATCH[1]}"
  fi
fi

if [[ -z "$base_url" || -z "$repo_full_name" ]]; then
  echo "Unable to infer forge base URL or repository name from origin remote." >&2
  exit 1
fi

if [[ "$dry_run" != "true" && -z "$token" ]]; then
  echo "A Gitea API token is required. Use --token or FORGE_RELEASE_TOKEN." >&2
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
  find "$release_root" -type f \
    \( -name '*.zip' -o -name '*.apk' -o -name 'RELEASE_NOTES.ko.md' -o -name 'SHA256SUMS.txt' -o -name 'version.json' \) \
    | sort
)

if [[ ${#asset_files[@]} -eq 0 ]]; then
  echo "No release assets found in $release_root" >&2
  exit 1
fi

echo "Forge release target: $base_url/$repo_full_name"
echo "Version: $version"
printf 'Assets:\n'
printf '  - %s\n' "${asset_files[@]#$release_root/}"

if [[ "$dry_run" == "true" ]]; then
  exit 0
fi

auth_header="Authorization: token $token"
tmp_response="$(mktemp)"
trap 'rm -f "$tmp_response"' EXIT

existing_status="$(curl -sS -o "$tmp_response" -w '%{http_code}' -H "$auth_header" "$tag_api")"
if [[ "$existing_status" == "200" ]]; then
  existing_release_id="$(python3 - <<'PY' "$tmp_response"
import json, sys
with open(sys.argv[1], "r", encoding="utf-8") as fh:
    print(json.load(fh)["id"])
PY
)"
  curl -sS -X DELETE -H "$auth_header" "${release_api}/${existing_release_id}" >/dev/null
fi

release_body=$'Windows와 Android 클라이언트 산출물을 병렬로 정리한 릴리즈입니다.\n\n'
release_body+=$'동일 버전 번호 아래 OS별 자산을 함께 게시하며, 최신 다운로드 채널은 download-vs-messanger.phy.kr에서 운영합니다.'

create_payload="$(python3 - <<'PY' "$version" "$release_body" "$pre_release"
import json, sys
version, body, prerelease = sys.argv[1], sys.argv[2], sys.argv[3] == "true"
print(json.dumps({
    "tag_name": version,
    "name": version,
    "body": body,
    "draft": False,
    "prerelease": prerelease
}, ensure_ascii=False))
PY
)"

create_status="$(curl -sS -o "$tmp_response" -w '%{http_code}' \
  -X POST \
  -H "$auth_header" \
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
    -H "$auth_header" \
    -H 'Content-Type: application/octet-stream' \
    --data-binary @"$asset" \
    "${release_api}/${release_id}/assets?name=${name}" >/dev/null
done

echo "Published forge release $version with ${#asset_files[@]} assets."
