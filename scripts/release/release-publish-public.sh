#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  ./scripts/release/release-publish-public.sh --version 2026.04.16-alpha.4 [options]

Options:
  --branch <name>          Source branch to publish. Default: public/main
  --target-branch <name>   Target branch name on public remotes. Default: main
  --stage-remote <name>    Stage remote. Default: public-stage
  --github-remote <name>   GitHub remote. Default: github-public
  --notes <path>           Release notes markdown file
  --skip-github            Publish to stage only
  --skip-stage             Publish to GitHub only
  --force-tag             Replace an existing local or remote tag
  --dry-run                Print the planned sequence only
EOF
}

version=""
branch_name="public/main"
target_branch="main"
stage_remote="public-stage"
github_remote="github-public"
notes_path=""
skip_github="false"
skip_stage="false"
force_tag="false"
dry_run="false"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      version="${2:-}"
      shift 2
      ;;
    --branch)
      branch_name="${2:-}"
      shift 2
      ;;
    --target-branch)
      target_branch="${2:-}"
      shift 2
      ;;
    --stage-remote)
      stage_remote="${2:-}"
      shift 2
      ;;
    --github-remote)
      github_remote="${2:-}"
      shift 2
      ;;
    --notes)
      notes_path="${2:-}"
      shift 2
      ;;
    --skip-github)
      skip_github="true"
      shift
      ;;
    --skip-stage)
      skip_stage="true"
      shift
      ;;
    --force-tag)
      force_tag="true"
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

if [[ -z "$notes_path" ]]; then
  notes_path="$repo_root/release-assets/releases/$version/RELEASE_NOTES.ko.md"
fi

target_commit="$(git rev-parse "$branch_name")"

echo "Version: $version"
echo "Source branch: $branch_name"
echo "Target branch: $target_branch"
echo "Target commit: $target_commit"
[[ "$skip_stage" != "true" ]] && echo "Stage remote: $stage_remote"
[[ "$skip_github" != "true" ]] && echo "GitHub remote: $github_remote"

tag_args=()
if [[ "$force_tag" == "true" ]]; then
  tag_args+=(--force)
fi

"$repo_root/scripts/release/release-create-tag.sh" \
  --version "$version" \
  --ref "$branch_name" \
  "${tag_args[@]}" \
  --dry-run

if [[ "$dry_run" == "true" ]]; then
  if [[ "$skip_stage" != "true" ]]; then
    echo "DRY RUN: ALLOW_PUBLIC_PUSH=1 git push $stage_remote refs/heads/$branch_name:refs/heads/$target_branch"
    echo "DRY RUN: ALLOW_PUBLIC_PUSH=1 git push $stage_remote refs/tags/$version:refs/tags/$version"
    "$repo_root/scripts/release/release-publish-forge.sh" \
      --remote "$stage_remote" \
      --version "$version" \
      --target-commitish "$target_branch" \
      --notes "$notes_path" \
      --dry-run
  fi

  if [[ "$skip_github" != "true" ]]; then
    echo "DRY RUN: ALLOW_PUBLIC_PUSH=1 git push $github_remote refs/heads/$branch_name:refs/heads/$target_branch"
    echo "DRY RUN: ALLOW_PUBLIC_PUSH=1 git push $github_remote refs/tags/$version:refs/tags/$version"
    "$repo_root/scripts/release/release-publish-github.sh" \
      --remote "$github_remote" \
      --version "$version" \
      --target-commitish "$target_branch" \
      --notes "$notes_path" \
      --dry-run
  fi
  exit 0
fi

"$repo_root/scripts/release/release-create-tag.sh" \
  --version "$version" \
  --ref "$branch_name" \
  "${tag_args[@]}"

push_branch() {
  local remote_name="$1"
  ALLOW_PUBLIC_PUSH=1 git push "$remote_name" "refs/heads/$branch_name:refs/heads/$target_branch"
}

push_tag() {
  local remote_name="$1"
  local force_args=()
  if [[ "$force_tag" == "true" ]]; then
    force_args+=(--force)
  fi
  ALLOW_PUBLIC_PUSH=1 git push "${force_args[@]}" "$remote_name" "refs/tags/$version:refs/tags/$version"
}

if [[ "$skip_stage" != "true" ]]; then
  push_branch "$stage_remote"
  push_tag "$stage_remote"
  "$repo_root/scripts/release/release-publish-forge.sh" \
    --remote "$stage_remote" \
    --version "$version" \
    --target-commitish "$target_branch" \
    --notes "$notes_path"
fi

if [[ "$skip_github" != "true" ]]; then
  push_branch "$github_remote"
  push_tag "$github_remote"
  "$repo_root/scripts/release/release-publish-github.sh" \
    --remote "$github_remote" \
    --version "$version" \
    --target-commitish "$target_branch" \
    --notes "$notes_path"
fi
