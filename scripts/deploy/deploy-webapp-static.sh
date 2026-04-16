#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  ./scripts/deploy/deploy-webapp-static.sh --host example.com --user deploy --source dist [options]

Options:
  --source <path>          Local webapp build directory to upload
  --version <name>         Remote release directory name. Default: current timestamp
  --app-dir <path>         Remote application root. Default: /srv/vs-messanger/app
  --target <path>          Remote webapp root. Default: /srv/vs-messanger/webapp
  --ssh-key <path>         Private key used for SSH/rsync
  --dry-run                Print the rsync plan without changing the server

Notes:
  - Remote host must already contain a valid deploy/.env file.
  - This script uploads static webapp files into releases/<version> and repoints current -> releases/<version>.
  - The webapp is expected to be served at https://vstalk.phy.kr via Caddy + compose.webapp.yml.
EOF
}

host=""
user=""
source_dir=""
version="$(date +%Y%m%d-%H%M%S)"
app_dir="/srv/vs-messanger/app"
target="/srv/vs-messanger/webapp"
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
    --source)
      source_dir="${2:-}"
      shift 2
      ;;
    --version)
      version="${2:-}"
      shift 2
      ;;
    --app-dir)
      app_dir="${2:-}"
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

if [[ -z "$host" || -z "$user" || -z "$source_dir" ]]; then
  usage >&2
  exit 1
fi

if [[ ! -d "$source_dir" ]]; then
  echo "Source directory not found: $source_dir" >&2
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
release_dir="$target/releases/$version"
current_link="$target/current"

"${ssh_cmd[@]}" "$target_host" "mkdir -p '$release_dir' '$target/releases' '$app_dir'"
rsync "${rsync_opts[@]}" -e "$rsh" "$source_dir"/ "$target_host:$release_dir/"

if [[ "$dry_run" == "true" ]]; then
  echo "Dry run complete. Remote symlink and compose were not updated."
  exit 0
fi

"${ssh_cmd[@]}" "$target_host" \
  "ln -sfn '$release_dir' '$current_link' && cd '$app_dir' && test -f deploy/.env && docker compose --env-file deploy/.env -f deploy/compose.mvp.yml -f deploy/compose.webapp.yml up -d webapp caddy"

echo "Deployed webapp release $version to $target_host:$release_dir"
