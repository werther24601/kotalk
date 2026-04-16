#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  ./scripts/deploy/deploy-stack-mvp.sh --host example.com --user deploy [options]

Options:
  --app-dir <path>         Remote application root. Default: /srv/vs-messanger/app
  --download-root <path>   Remote download root. Default: /srv/vs-messanger/download
  --ssh-key <path>         Private key used for SSH/rsync
  --dry-run                Print the rsync plan without changing the server

Notes:
  - Remote host must already contain a valid deploy/.env file.
  - This script syncs deploy files and the current src tree, then runs docker compose.
EOF
}

host=""
user=""
app_dir="/srv/vs-messanger/app"
download_root="/srv/vs-messanger/download"
ssh_key=""
dry_run="false"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --host)
      host="${2:-}"
      shift 2
      ;;
    --user)
      user="${2:-}"
      shift 2
      ;;
    --app-dir)
      app_dir="${2:-}"
      shift 2
      ;;
    --download-root)
      download_root="${2:-}"
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

if [[ -z "$host" || -z "$user" ]]; then
  usage >&2
  exit 1
fi

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
ssh_cmd=(ssh -o StrictHostKeyChecking=accept-new)
if [[ -n "$ssh_key" ]]; then
  ssh_cmd+=(-i "$ssh_key")
fi

rsync_opts=(-az)
if [[ "$dry_run" == "true" ]]; then
  rsync_opts+=(--dry-run)
fi

target_host="$user@$host"
rsh="${ssh_cmd[*]}"

"${ssh_cmd[@]}" "$target_host" "mkdir -p '$app_dir' '$download_root'"

rsync "${rsync_opts[@]}" \
  -e "$rsh" \
  --delete \
  --filter="protect /deploy/.env" \
  --include "/VsMessenger.sln" \
  --include "/global.json" \
  --include "/deploy/***" \
  --include "/src/***" \
  --exclude "*" \
  "$repo_root"/ "$target_host:$app_dir/"

if [[ "$dry_run" == "true" ]]; then
  echo "Dry run complete. Remote compose was not started."
  exit 0
fi

"${ssh_cmd[@]}" "$target_host" \
  "cd '$app_dir' && test -f deploy/.env && docker compose --env-file deploy/.env -f deploy/compose.mvp.yml up -d --build --remove-orphans"

echo "Deployed MVP stack to $target_host:$app_dir"
