#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  ./scripts/release/build-windows-distributions.sh --version 2026.04.16-alpha.6 [options]

Options:
  --configuration <name>   Build configuration. Default: Release
  --runtime <rid>          Runtime identifier. Default: win-x64
  --output <dir>           Output directory. Default: artifacts/builds/<version>
  --dotnet <path>          Explicit dotnet binary path
EOF
}

version=""
configuration="Release"
runtime="win-x64"
output_dir=""
dotnet_bin="${DOTNET_BIN:-}"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      version="${2:-}"
      shift 2
      ;;
    --configuration)
      configuration="${2:-}"
      shift 2
      ;;
    --runtime)
      runtime="${2:-}"
      shift 2
      ;;
    --output)
      output_dir="${2:-}"
      shift 2
      ;;
    --dotnet)
      dotnet_bin="${2:-}"
      shift 2
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

if [[ -z "$dotnet_bin" ]]; then
  if command -v dotnet >/dev/null 2>&1; then
    dotnet_bin="$(command -v dotnet)"
  elif [[ -x "$HOME/.dotnet/dotnet" ]]; then
    dotnet_bin="$HOME/.dotnet/dotnet"
  else
    echo "Unable to find dotnet. Set --dotnet or DOTNET_BIN." >&2
    exit 1
  fi
fi

if [[ -z "$output_dir" ]]; then
  output_dir="$repo_root/artifacts/builds/$version"
fi

publish_dir="$output_dir/publish/$runtime"
onefile_dir="$output_dir/onefile/$runtime"
zip_path="$output_dir/KoTalk-windows-x64-$version.zip"
portable_exe_path="$output_dir/KoTalk-windows-x64-onefile-$version.exe"
installer_path="$output_dir/KoTalk-windows-x64-installer-$version.exe"
installer_script="$repo_root/packaging/windows/KoTalkInstaller.nsi"
app_icon="$repo_root/branding/ico/kotalk.ico"

rm -rf "$publish_dir" "$onefile_dir"
mkdir -p "$publish_dir" "$onefile_dir" "$output_dir"

"$dotnet_bin" publish "$repo_root/src/PhysOn.Desktop/PhysOn.Desktop.csproj" \
  -c "$configuration" \
  -r "$runtime" \
  --self-contained true \
  -p:DebugSymbols=false \
  -p:DebugType=None \
  -o "$publish_dir"

rm -f "$zip_path"
(cd "$publish_dir" && zip -rq "$zip_path" .)

"$dotnet_bin" publish "$repo_root/src/PhysOn.Desktop/PhysOn.Desktop.csproj" \
  -c "$configuration" \
  -r "$runtime" \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true \
  -p:DebugSymbols=false \
  -p:DebugType=None \
  -o "$onefile_dir"

cp "$onefile_dir/KoTalk.exe" "$portable_exe_path"

makensis \
  -DAPP_VERSION="$version" \
  -DAPP_ICON="$app_icon" \
  -DSOURCE_DIR="$publish_dir" \
  -DOUTPUT_FILE="$installer_path" \
  "$installer_script" >/dev/null

(
  cd "$output_dir"
  sha256sum \
    "$(basename "$zip_path")" \
    "$(basename "$portable_exe_path")" \
    "$(basename "$installer_path")" \
    > SHA256SUMS.txt
)

echo "Built Windows release assets in $output_dir"
