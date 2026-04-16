#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
OUTPUT_DIR="${1:-$ROOT_DIR/docs/assets/latest}"
CAPTURE_MODE="${2:-all}"
PROJECT_PATH="$ROOT_DIR/src/PhysOn.Desktop/PhysOn.Desktop.csproj"
DOTNET_BIN="${DOTNET_BIN:-$HOME/.dotnet/dotnet}"

if [[ ! -x "$DOTNET_BIN" ]]; then
  echo "dotnet not found at $DOTNET_BIN" >&2
  exit 1
fi

mkdir -p "$OUTPUT_DIR"
TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

capture_mode() {
  local mode="$1"
  local output_path="$2"
  local main_title="$3"
  local detached_title="${4:-}"
  local data_home="$TMP_DIR/$mode-data"
  local config_home="$TMP_DIR/$mode-config"
  local cache_home="$TMP_DIR/$mode-cache"
  local runtime_home="$TMP_DIR/$mode-runtime"
  local tree_path="$TMP_DIR/$mode-tree.txt"
  local log_path="$TMP_DIR/$mode.log"
  mkdir -p "$data_home" "$config_home" "$cache_home" "$runtime_home"

  env \
    XDG_DATA_HOME="$data_home" \
    XDG_CONFIG_HOME="$config_home" \
    XDG_CACHE_HOME="$cache_home" \
    XDG_RUNTIME_DIR="$runtime_home" \
    DOTNET_BIN="$DOTNET_BIN" \
    PROJECT_PATH="$PROJECT_PATH" \
    TREE_PATH="$tree_path" \
    LOG_PATH="$log_path" \
    OUTPUT_PATH="$output_path" \
    MAIN_TITLE="$main_title" \
    DETACHED_TITLE="$detached_title" \
    MODE="$mode" \
    xvfb-run -a bash -lc '
      set -euo pipefail

      refresh_tree() {
        xwininfo -root -tree >"$TREE_PATH" 2>/dev/null || true
      }

      find_window_id() {
        local title="$1"
        python3 - "$TREE_PATH" "$title" <<'"'"'PY'"'"'
import re
import sys

tree_path, title = sys.argv[1], sys.argv[2]
pattern = re.compile(r"^\s*(0x[0-9a-fA-F]+)\s+\"([^\"]+)\"")

with open(tree_path, "r", encoding="utf-8", errors="ignore") as handle:
    for line in handle:
        match = pattern.match(line)
        if not match:
            continue
        window_id, window_title = match.groups()
        if window_title == title:
            print(window_id)
            raise SystemExit(0)

raise SystemExit(1)
PY
      }

      wait_for_window() {
        local title="$1"
        local attempts="${2:-40}"
        local pause="${3:-0.5}"
        local window_id=""

        for _ in $(seq 1 "$attempts"); do
          refresh_tree
          if window_id="$(find_window_id "$title" 2>/dev/null)"; then
            echo "$window_id"
            return 0
          fi
          sleep "$pause"
        done

        return 1
      }

      window_geometry() {
        local window_id="$1"
        python3 - "$window_id" <<'"'"'PY'"'"'
import re
import subprocess
import sys

window_id = sys.argv[1]
output = subprocess.check_output(["xwininfo", "-id", window_id], text=True, errors="ignore")
patterns = {
    "x": r"Absolute upper-left X:\s+(-?\d+)",
    "y": r"Absolute upper-left Y:\s+(-?\d+)",
    "w": r"Width:\s+(\d+)",
    "h": r"Height:\s+(\d+)",
}

values = {}
for key, pattern in patterns.items():
    match = re.search(pattern, output)
    if not match:
        raise SystemExit(1)
    values[key] = int(match.group(1))

if values["w"] < 240 or values["h"] < 240:
    raise SystemExit(2)

print(values["x"], values["y"], values["w"], values["h"])
PY
      }

      wait_for_geometry() {
        local window_id="$1"
        local attempts="${2:-30}"
        local pause="${3:-0.4}"
        local geometry=""
        local previous=""

        for _ in $(seq 1 "$attempts"); do
          if geometry="$(window_geometry "$window_id" 2>/dev/null)"; then
            if [[ "$geometry" == "$previous" ]]; then
              echo "$geometry"
              return 0
            fi
            previous="$geometry"
          fi
          sleep "$pause"
        done

        if [[ -n "$previous" ]]; then
          echo "$previous"
          return 0
        fi

        return 1
      }

      capture_window() {
        local window_id="$1"
        local target_path="$2"
        wait_for_geometry "$window_id" >/dev/null
        import -window "$window_id" "$target_path"
      }

      create_conversation_fallback() {
        local source_path="$1"
        local target_path="$2"
        convert "$source_path" -gravity east -crop 58%x84%+0+0 +repage "$target_path"
      }

      if [[ "$MODE" == "sample" ]]; then
        export KOTALK_DESKTOP_SAMPLE_MODE=1
        export KOTALK_DESKTOP_OPEN_SAMPLE_WINDOW=1
      fi
      export XDG_DATA_HOME="$XDG_DATA_HOME"
      export XDG_CONFIG_HOME="$XDG_CONFIG_HOME"
      export XDG_CACHE_HOME="$XDG_CACHE_HOME"
      export XDG_RUNTIME_DIR="$XDG_RUNTIME_DIR"

      "$DOTNET_BIN" run --project "$PROJECT_PATH" -c Debug >"$LOG_PATH" 2>&1 &
      app_pid=$!

      cleanup() {
        kill "$app_pid" >/dev/null 2>&1 || true
        wait "$app_pid" >/dev/null 2>&1 || true
      }
      trap cleanup EXIT

      main_id="$(wait_for_window "$MAIN_TITLE" 60 0.5)"
      capture_window "$main_id" "$OUTPUT_PATH"

      if [[ -n "$DETACHED_TITLE" ]]; then
        conversation_output="${OUTPUT_PATH%/*}/conversation.${OUTPUT_PATH##*.}"
        if detached_id="$(wait_for_window "$DETACHED_TITLE" 12 0.5 2>/dev/null)"; then
          capture_window "$detached_id" "$conversation_output"
        else
          create_conversation_fallback "$OUTPUT_PATH" "$conversation_output"
        fi
      fi
    '
}

if [[ "$CAPTURE_MODE" == "all" || "$CAPTURE_MODE" == "onboarding" ]]; then
  capture_mode "onboarding" "$OUTPUT_DIR/onboarding.png" "KoTalk"
fi

if [[ "$CAPTURE_MODE" == "all" || "$CAPTURE_MODE" == "sample" ]]; then
  capture_mode "sample" "$OUTPUT_DIR/hero-shell.png" "KoTalk" "제품 운영"
fi

echo "Desktop screenshots written to $OUTPUT_DIR"
