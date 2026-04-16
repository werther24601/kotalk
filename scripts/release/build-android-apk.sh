#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  ./scripts/release/build-android-apk.sh --version 2026.04.16-alpha.6 [options]

Options:
  --configuration <name>   Build configuration. Default: Release
  --output <dir>           Output directory. Default: artifacts/builds/<version>
  --dotnet <path>          Explicit dotnet binary path
EOF
}

version=""
configuration="Release"
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

if [[ -z "${JAVA_HOME:-}" && -d /usr/lib/jvm/java-17-openjdk-amd64 ]]; then
  export JAVA_HOME=/usr/lib/jvm/java-17-openjdk-amd64
  export PATH="$JAVA_HOME/bin:$PATH"
fi

if [[ -z "${ANDROID_SDK_ROOT:-}" ]]; then
  export ANDROID_SDK_ROOT="$HOME/Android/Sdk"
fi

if [[ -z "$output_dir" ]]; then
  output_dir="$repo_root/artifacts/builds/$version"
fi

publish_dir="$output_dir/android/publish"
apk_path="$output_dir/KoTalk-android-universal-$version.apk"

rm -rf "$publish_dir"
mkdir -p "$publish_dir" "$output_dir"

if [[ ! -d "$ANDROID_SDK_ROOT/platforms" ]]; then
  mkdir -p "$ANDROID_SDK_ROOT"
  "$dotnet_bin" build "$repo_root/src/PhysOn.Mobile.Android/PhysOn.Mobile.Android.csproj" \
    -t:InstallAndroidDependencies \
    -f net8.0-android \
    -p:AndroidSdkDirectory="$ANDROID_SDK_ROOT" \
    -p:JavaSdkDirectory="${JAVA_HOME:-}" >/dev/null
fi

"$dotnet_bin" publish "$repo_root/src/PhysOn.Mobile.Android/PhysOn.Mobile.Android.csproj" \
  -c "$configuration" \
  -f net8.0-android \
  -p:AndroidPackageFormat=apk \
  -p:AndroidKeyStore=false \
  -p:AndroidSdkDirectory="$ANDROID_SDK_ROOT" \
  -p:JavaSdkDirectory="${JAVA_HOME:-}" \
  -o "$publish_dir"

apk_source="$(find "$publish_dir" -type f -name '*.apk' | head -n 1)"
if [[ -z "$apk_source" ]]; then
  echo "Android publish did not produce an APK." >&2
  exit 1
fi

cp "$apk_source" "$apk_path"

(
  cd "$output_dir"
  sha256sum "$(basename "$apk_path")" > KoTalk-android-universal-$version.apk.sha256
)

echo "Built Android APK in $output_dir"
