#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  ./scripts/release/release-upload-assets.sh --version v0.2.0-alpha.1 --host example.com --user deploy [options]

Options:
  --target <path>          Remote download root. Default: /srv/vs-messanger/download
  --ssh-key <path>         Private key used for SSH/rsync
  --dry-run                Print the rsync plan without changing the server
EOF
}

version=""
host=""
user=""
target="/srv/vs-messanger/download"
ssh_key=""
dry_run="false"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      version="${2:-}"
      shift 2
      ;;
    --host)
      host="${2:-}"
      shift 2
      ;;
    --user)
      user="${2:-}"
      shift 2
      ;;
    --target)
      target="${2:-}"
      shift 2
      ;;
    --ssh-key)
      ssh_key="${2:-}"
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

if [[ -z "$version" || -z "$host" || -z "$user" ]]; then
  usage >&2
  exit 1
fi

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
release_root="$repo_root/release-assets/releases/$version"
latest_root="$repo_root/release-assets/latest"
download_root="$repo_root/release-assets/root"

if [[ ! -d "$release_root" || ! -d "$latest_root" ]]; then
  echo "Prepared release bundle not found for version $version" >&2
  exit 1
fi

ssh_cmd=(ssh -o StrictHostKeyChecking=accept-new)
if [[ -n "$ssh_key" ]]; then
  ssh_cmd+=(-i "$ssh_key")
fi

rsync_opts=(-az)
if [[ "$dry_run" == "true" ]]; then
  rsync_opts+=(--dry-run)
else
  rsync_opts+=(--delete)
fi

target_host="$user@$host"
rsh="${ssh_cmd[*]}"

"${ssh_cmd[@]}" "$target_host" "mkdir -p '$target/releases/$version' '$target/latest' '$target/windows/latest' '$target/android/latest'"
rsync "${rsync_opts[@]}" -e "$rsh" "$release_root"/ "$target_host:$target/releases/$version/"
rsync "${rsync_opts[@]}" -e "$rsh" "$latest_root"/ "$target_host:$target/latest/"
if [[ -f "$download_root/index.html" ]]; then
  root_rsync_opts=(-az)
  if [[ "$dry_run" == "true" ]]; then
    root_rsync_opts+=(--dry-run)
  fi
  rsync "${root_rsync_opts[@]}" -e "$rsh" "$download_root/index.html" "$target_host:$target/index.html"
fi
if [[ -d "$latest_root/windows" ]]; then
  rsync "${rsync_opts[@]}" -e "$rsh" "$latest_root/windows/" "$target_host:$target/windows/latest/"
fi
if [[ -d "$latest_root/android" ]]; then
  rsync "${rsync_opts[@]}" -e "$rsh" "$latest_root/android/" "$target_host:$target/android/latest/"
fi

echo "Uploaded release assets for $version to $target_host:$target"
