#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  ./scripts/release/release-prepare-assets.sh --version v0.2.0-alpha.1 [options]

Options:
  --windows-zip <path>     Windows x64 ZIP artifact path
  --android-apk <path>     Android universal APK artifact path
  --zip <path>             Backward-compatible alias for --windows-zip
  --channel <name>         Release channel. Default: alpha
  --notes <path>           Existing Korean release notes file
  --screenshots <dir>      Directory containing *.png/jpg screenshots
  --force                  Overwrite an existing release folder

Environment:
  DOWNLOAD_BASE_URL        Defaults to https://download-vstalk.phy.kr
EOF
}

version=""
channel="alpha"
windows_zip=""
android_apk=""
notes_path=""
screenshots_dir=""
force="false"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      version="${2:-}"
      shift 2
      ;;
    --channel)
      channel="${2:-}"
      shift 2
      ;;
    --windows-zip|--zip)
      windows_zip="${2:-}"
      shift 2
      ;;
    --android-apk)
      android_apk="${2:-}"
      shift 2
      ;;
    --notes)
      notes_path="${2:-}"
      shift 2
      ;;
    --screenshots)
      screenshots_dir="${2:-}"
      shift 2
      ;;
    --force)
      force="true"
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

if [[ -z "$windows_zip" && -z "$android_apk" ]]; then
  echo "At least one artifact must be provided: --windows-zip or --android-apk" >&2
  usage >&2
  exit 1
fi

if [[ -n "$windows_zip" && ! -f "$windows_zip" ]]; then
  echo "Windows ZIP artifact not found: $windows_zip" >&2
  exit 1
fi

if [[ -n "$android_apk" && ! -f "$android_apk" ]]; then
  echo "Android APK artifact not found: $android_apk" >&2
  exit 1
fi

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
release_root="$repo_root/release-assets/releases/$version"
latest_root="$repo_root/release-assets/latest"
download_root="$repo_root/release-assets/root"
template_path="$repo_root/release-assets/templates/RELEASE_NOTES.ko.md"
download_base_url="${DOWNLOAD_BASE_URL:-https://download-vstalk.phy.kr}"
release_base_url="${RELEASE_BASE_URL:-}"
published_at="$(date -u +"%Y-%m-%dT%H:%M:%SZ")"

derive_release_url() {
  if [[ -n "$release_base_url" ]]; then
    printf '%s/releases/tag/%s' "${release_base_url%/}" "$version"
    return 0
  fi

  local origin_url
  origin_url="$(git -C "$repo_root" remote get-url origin 2>/dev/null || true)"

  if [[ -z "$origin_url" ]]; then
    return 0
  fi

  if [[ "$origin_url" =~ ^https?:// ]]; then
    printf '%s/releases/tag/%s' "${origin_url%.git}" "$version"
    return 0
  fi

  if [[ "$origin_url" =~ ^git@([^:]+):(.+)\.git$ ]]; then
    printf 'https://%s/%s/releases/tag/%s' "${BASH_REMATCH[1]}" "${BASH_REMATCH[2]}" "$version"
    return 0
  fi
}

release_url="$(derive_release_url)"

if [[ -e "$release_root" && "$force" != "true" ]]; then
  echo "Release directory already exists: $release_root" >&2
  echo "Use --force to replace it." >&2
  exit 1
fi

rm -rf "$release_root" "$latest_root" "$download_root"
mkdir -p "$release_root/screenshots" "$latest_root/screenshots" "$download_root"

if [[ -n "$notes_path" ]]; then
  cp "$notes_path" "$release_root/RELEASE_NOTES.ko.md"
else
  sed \
    -e "s/{{VERSION}}/$version/g" \
    -e "s/{{CHANNEL}}/$channel/g" \
    -e "s/{{PUBLISHED_AT}}/$published_at/g" \
    "$template_path" > "$release_root/RELEASE_NOTES.ko.md"
fi
cp "$release_root/RELEASE_NOTES.ko.md" "$latest_root/RELEASE_NOTES.ko.md"

if [[ -n "$screenshots_dir" ]]; then
  while IFS= read -r screenshot; do
    cp "$screenshot" "$release_root/screenshots/$(basename "$screenshot")"
    cp "$screenshot" "$latest_root/screenshots/$(basename "$screenshot")"
  done < <(find "$screenshots_dir" -maxdepth 1 -type f \( -iname '*.png' -o -iname '*.jpg' -o -iname '*.jpeg' \) | sort)
fi

platform_count=0
platforms_json=""
top_level_windows_alias=""
release_hash_paths=()
latest_hash_paths=()

append_platform_json() {
  local body="$1"
  if (( platform_count > 0 )); then
    platforms_json+=$',\n'
  fi
  platforms_json+="$body"
  platform_count=$((platform_count + 1))
}

write_platform_version_json() {
  local path="$1"
  local body="$2"
  cat > "$path" <<EOF
{
  "productName": "KoTalk",
  "publisher": "PHYSIA",
  "version": "$version",
  "channel": "$channel",
  "publishedAt": "$published_at",
  "notesUrl": "$download_base_url/releases/$version/RELEASE_NOTES.ko.md",
  "releaseUrl": "$release_url",
  "platform": {
$body
  }
}
EOF
}

if [[ -n "$windows_zip" ]]; then
  windows_release_name="KoTalk-windows-x64-$version.zip"
  windows_latest_name="KoTalk-windows-x64.zip"
  windows_release_dir="$release_root/windows/x64"
  windows_latest_dir="$latest_root/windows"
  mkdir -p "$windows_release_dir" "$windows_latest_dir"

  cp "$windows_zip" "$windows_release_dir/$windows_release_name"
  cp "$windows_zip" "$windows_latest_dir/$windows_latest_name"

  (
    cd "$windows_release_dir"
    sha256sum "$windows_release_name" > SHA256SUMS.txt
  )

  (
    cd "$windows_latest_dir"
    sha256sum "$windows_latest_name" > SHA256SUMS.txt
  )

  release_hash_paths+=("windows/x64/$windows_release_name")
  latest_hash_paths+=("windows/$windows_latest_name")

  windows_platform_body="$(cat <<EOF
    "name": "KoTalk for Windows",
    "kind": "desktop",
    "arch": "x64",
    "latestUrl": "$download_base_url/windows/latest",
    "portableZipUrl": "$download_base_url/windows/latest/$windows_latest_name",
    "sha256Url": "$download_base_url/windows/latest/SHA256SUMS.txt"
EOF
)"

  append_platform_json "$(cat <<EOF
    "windows": {
$windows_platform_body
    }
EOF
)"

  top_level_windows_alias="$(cat <<EOF
,
  "windows": {
$windows_platform_body
  }
EOF
)"

  write_platform_version_json "$windows_latest_dir/version.json" "$windows_platform_body"
fi

if [[ -n "$android_apk" ]]; then
  android_release_name="KoTalk-android-universal-$version.apk"
  android_latest_name="KoTalk-android-universal.apk"
  android_release_dir="$release_root/android/universal"
  android_latest_dir="$latest_root/android"
  mkdir -p "$android_release_dir" "$android_latest_dir"

  cp "$android_apk" "$android_release_dir/$android_release_name"
  cp "$android_apk" "$android_latest_dir/$android_latest_name"

  (
    cd "$android_release_dir"
    sha256sum "$android_release_name" > SHA256SUMS.txt
  )

  (
    cd "$android_latest_dir"
    sha256sum "$android_latest_name" > SHA256SUMS.txt
  )

  release_hash_paths+=("android/universal/$android_release_name")
  latest_hash_paths+=("android/$android_latest_name")

  android_platform_body="$(cat <<EOF
    "name": "KoTalk for Android",
    "kind": "mobile",
    "arch": "universal",
    "packageName": "kr.physia.kotalk",
    "minSdk": 26,
    "latestUrl": "$download_base_url/android/latest",
    "apkUrl": "$download_base_url/android/latest/$android_latest_name",
    "sha256Url": "$download_base_url/android/latest/SHA256SUMS.txt"
EOF
)"

  append_platform_json "$(cat <<EOF
    "android": {
$android_platform_body
    }
EOF
)"

  write_platform_version_json "$android_latest_dir/version.json" "$android_platform_body"
fi

if (( ${#release_hash_paths[@]} > 0 )); then
  (
    cd "$release_root"
    sha256sum "${release_hash_paths[@]}" > SHA256SUMS.txt
  )
fi

if (( ${#latest_hash_paths[@]} > 0 )); then
  (
    cd "$latest_root"
    sha256sum "${latest_hash_paths[@]}" > SHA256SUMS.txt
  )
fi

windows_landing_card=""
if [[ -n "$windows_zip" ]]; then
  windows_landing_card="$(cat <<EOF
      <a class="card" href="/windows/latest">
        <span class="eyebrow">Windows</span>
        <strong>Latest Windows build</strong>
        <span>ZIP package and SHA256 checksum</span>
      </a>
EOF
)"
fi

android_landing_card=""
if [[ -n "$android_apk" ]]; then
  android_landing_card="$(cat <<EOF
      <a class="card" href="/android/latest">
        <span class="eyebrow">Android</span>
        <strong>Latest Android build</strong>
        <span>Universal APK and SHA256 checksum</span>
      </a>
EOF
)"
fi

cat > "$download_root/index.html" <<EOF
<!doctype html>
<html lang="ko">
  <head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>KoTalk Downloads</title>
    <style>
      :root {
        color-scheme: light;
        --bg: #f7f3ee;
        --surface: #ffffff;
        --surface-muted: #f1ebe4;
        --border: #ddd1c4;
        --text: #20242b;
        --text-soft: #5f5a54;
        --accent: #f05b2b;
        --accent-soft: #394350;
      }
      * { box-sizing: border-box; }
      body {
        margin: 0;
        min-height: 100vh;
        font-family: Inter, "Segoe UI", system-ui, -apple-system, sans-serif;
        background: var(--bg);
        color: var(--text);
      }
      main {
        max-width: 920px;
        margin: 0 auto;
        padding: 48px 24px 64px;
      }
      .hero {
        background: var(--surface);
        border: 1px solid var(--border);
        padding: 24px;
      }
      h1 {
        margin: 0 0 10px;
        font-size: 34px;
        line-height: 1.1;
        letter-spacing: -0.04em;
      }
      p {
        margin: 0;
        color: var(--text-soft);
        line-height: 1.6;
      }
      .grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
        gap: 16px;
        margin-top: 20px;
      }
      .card {
        display: flex;
        flex-direction: column;
        gap: 8px;
        text-decoration: none;
        color: inherit;
        background: var(--surface);
        border: 1px solid var(--border);
        padding: 18px;
      }
      .card:hover {
        border-color: var(--accent);
      }
      .eyebrow {
        color: var(--accent-soft);
        font-size: 12px;
        font-weight: 700;
        text-transform: uppercase;
        letter-spacing: 0.08em;
      }
      .meta {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
        gap: 12px;
        margin-top: 20px;
      }
      .meta a {
        color: var(--accent-soft);
      }
      @media (max-width: 640px) {
        main { padding: 24px 16px 40px; }
        h1 { font-size: 28px; }
      }
    </style>
  </head>
  <body>
    <main>
      <section class="hero">
        <h1>KoTalk Downloads</h1>
        <p>KoTalk의 최신 배포 파일과 버전 메타데이터를 제공하는 공식 다운로드 표면입니다.</p>
        <div class="grid">
$windows_landing_card
$android_landing_card
          <a class="card" href="/latest/version.json">
            <span class="eyebrow">Manifest</span>
            <strong>Version manifest</strong>
            <span>Current release metadata and asset URLs</span>
          </a>
        </div>
        <div class="meta">
          <p><strong>Version</strong><br>$version</p>
          <p><strong>Channel</strong><br>$channel</p>
          <p><strong>Published</strong><br>$published_at</p>
          <p><strong>Latest notes</strong><br><a href="/latest/RELEASE_NOTES.ko.md">RELEASE_NOTES.ko.md</a></p>
        </div>
      </section>
    </main>
  </body>
</html>
EOF

mapfile -t screenshot_files < <(find "$release_root/screenshots" -maxdepth 1 -type f \( -iname '*.png' -o -iname '*.jpg' -o -iname '*.jpeg' \) | sort)

screenshots_json="[]"
if [[ ${#screenshot_files[@]} -gt 0 ]]; then
  screenshots_json=$(
    for idx in "${!screenshot_files[@]}"; do
      name="$(basename "${screenshot_files[$idx]}")"
      printf '    "%s/releases/%s/screenshots/%s"' "$download_base_url" "$version" "$name"
      if (( idx < ${#screenshot_files[@]} - 1 )); then
        printf ',\n'
      else
        printf '\n'
      fi
    done
  )
  screenshots_json="[
$screenshots_json
  ]"
fi

cat > "$release_root/version.json" <<EOF
{
  "productName": "KoTalk",
  "publisher": "PHYSIA",
  "version": "$version",
  "channel": "$channel",
  "publishedAt": "$published_at",
  "notesUrl": "$download_base_url/releases/$version/RELEASE_NOTES.ko.md",
  "releaseUrl": "$release_url",
  "platforms": {
$platforms_json
  },
  "screenshots": $screenshots_json$top_level_windows_alias
}
EOF

cp "$release_root/version.json" "$latest_root/version.json"
cp "$release_root/version.json" "$latest_root/latest.json"
touch "$latest_root/.gitkeep"

echo "Prepared release bundle:"
echo "  release-assets/releases/$version"
echo "  release-assets/latest"
echo "  release-assets/root"
