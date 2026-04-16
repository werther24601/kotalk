#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  ./scripts/release/release-create-tag.sh --version 2026.04.16-alpha.4 [options]

Options:
  --ref <git-ref>          Target ref or commit. Default: public/main
  --message <text>         Annotated tag message
  --remote <name>          Push tag to a remote after creation
  --force                  Replace an existing local tag
  --dry-run                Print what would happen
EOF
}

version=""
target_ref="public/main"
message=""
remote_name=""
force="false"
dry_run="false"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      version="${2:-}"
      shift 2
      ;;
    --ref)
      target_ref="${2:-}"
      shift 2
      ;;
    --message)
      message="${2:-}"
      shift 2
      ;;
    --remote)
      remote_name="${2:-}"
      shift 2
      ;;
    --force)
      force="true"
      shift
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
cd "$repo_root"

target_commit="$(git rev-parse "$target_ref")"
tag_message="${message:-Release $version}"

echo "Tag: $version"
echo "Target ref: $target_ref"
echo "Target commit: $target_commit"
[[ -n "$remote_name" ]] && echo "Remote: $remote_name"

if [[ "$dry_run" == "true" ]]; then
  exit 0
fi

if git rev-parse -q --verify "refs/tags/$version" >/dev/null; then
  if [[ "$force" != "true" ]]; then
    echo "Tag already exists: $version" >&2
    echo "Use --force to replace it." >&2
    exit 1
  fi
  git tag -d "$version" >/dev/null
fi

git tag -a "$version" "$target_commit" -m "$tag_message"

if [[ -n "$remote_name" ]]; then
  push_args=()
  if [[ "$force" == "true" ]]; then
    push_args+=(--force)
  fi
  git push "${push_args[@]}" "$remote_name" "refs/tags/$version:refs/tags/$version"
fi

echo "Created annotated tag $version at $target_commit"
