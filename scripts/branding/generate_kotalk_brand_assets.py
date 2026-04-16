#!/usr/bin/env python3

from __future__ import annotations

import math
import subprocess
from pathlib import Path

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[2]
BRANDING_DIR = ROOT / "branding"
REFERENCE_DIR = BRANDING_DIR / "reference"
PNG_DIR = BRANDING_DIR / "png"
ICO_DIR = BRANDING_DIR / "ico"
WEB_PUBLIC_DIR = ROOT / "src" / "PhysOn.Web" / "public"
DESKTOP_ASSETS_DIR = ROOT / "src" / "PhysOn.Desktop" / "Assets"

ARTBOARD = 1024
DARK = "#394350"
WARM = "#F05B2B"
WHITE = "#FFFFFF"
PAPER = "#F7F3EE"
NIGHT = "#141922"
BLACK = "#111111"

LEFT_RECT = (218, 312, 584, 602, 22)
LEFT_TAIL = [(304, 602), (304, 736), (438, 602)]
RIGHT_RECT = (446, 312, 812, 602, 22)
RIGHT_TAIL = [(668, 602), (742, 602), (742, 694), (694, 650)]
CHEVRON = [(490, 328), (582, 328), (446, 457), (582, 586), (490, 586), (338, 457)]


def ensure_dirs() -> None:
    for path in (BRANDING_DIR, REFERENCE_DIR, PNG_DIR, ICO_DIR, WEB_PUBLIC_DIR, DESKTOP_ASSETS_DIR):
        path.mkdir(parents=True, exist_ok=True)


def hex_to_rgba(hex_value: str, alpha: int = 255) -> tuple[int, int, int, int]:
    hex_value = hex_value.lstrip("#")
    return tuple(int(hex_value[index : index + 2], 16) for index in (0, 2, 4)) + (alpha,)


def rounded_rect_path(x: float, y: float, width: float, height: float, radius: float) -> str:
    right = x + width
    bottom = y + height
    return (
        f"M {x + radius:.2f} {y:.2f} "
        f"H {right - radius:.2f} "
        f"A {radius:.2f} {radius:.2f} 0 0 1 {right:.2f} {y + radius:.2f} "
        f"V {bottom - radius:.2f} "
        f"A {radius:.2f} {radius:.2f} 0 0 1 {right - radius:.2f} {bottom:.2f} "
        f"H {x + radius:.2f} "
        f"A {radius:.2f} {radius:.2f} 0 0 1 {x:.2f} {bottom - radius:.2f} "
        f"V {y + radius:.2f} "
        f"A {radius:.2f} {radius:.2f} 0 0 1 {x + radius:.2f} {y:.2f} Z"
    )


def polygon_path(points: list[tuple[float, float]]) -> str:
    start_x, start_y = points[0]
    segments = [f"M {start_x:.2f} {start_y:.2f}"]
    for x, y in points[1:]:
        segments.append(f"L {x:.2f} {y:.2f}")
    segments.append("Z")
    return " ".join(segments)


def svg_document(
    *,
    background: str | None,
    left_fill: str,
    right_fill: str,
    chevron_fill: str,
) -> str:
    background_markup = (
        f'<path d="{rounded_rect_path(0, 0, ARTBOARD, ARTBOARD, 0)}" fill="{background}" />\n  '
        if background
        else ""
    )
    return f"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {ARTBOARD} {ARTBOARD}" fill="none">
  {background_markup}<path d="{rounded_rect_path(*LEFT_RECT)}" fill="{left_fill}" />
  <path d="{polygon_path(LEFT_TAIL)}" fill="{left_fill}" />
  <path d="{rounded_rect_path(*RIGHT_RECT)}" fill="{right_fill}" />
  <path d="{polygon_path(RIGHT_TAIL)}" fill="{right_fill}" />
  <path d="{polygon_path(CHEVRON)}" fill="{chevron_fill}" />
</svg>
"""


def ps_color(hex_value: str) -> str:
    r, g, b, _ = hex_to_rgba(hex_value)
    return f"{r / 255:.6f} {g / 255:.6f} {b / 255:.6f} setrgbcolor"


def ps_polygon(points: list[tuple[float, float]]) -> str:
    lines = ["newpath"]
    start_x, start_y = points[0]
    lines.append(f"{start_x:.2f} {start_y:.2f} moveto")
    for x, y in points[1:]:
        lines.append(f"{x:.2f} {y:.2f} lineto")
    lines.append("closepath fill")
    return "\n".join(lines)


def eps_document() -> str:
    x1, y1, x2, y2, r = LEFT_RECT
    x1b, y1b, x2b, y2b, rb = RIGHT_RECT
    return f"""%!PS-Adobe-3.0 EPSF-3.0
%%BoundingBox: 0 0 {ARTBOARD} {ARTBOARD}
%%HiResBoundingBox: 0 0 {ARTBOARD} {ARTBOARD}
%%LanguageLevel: 2
%%Pages: 1
%%EndComments
/roundrect {{
  /r exch def
  /y2 exch def
  /x2 exch def
  /y exch def
  /x exch def
  newpath
  x r add y moveto
  x2 r sub y lineto
  x2 r sub y r add r 270 360 arc
  x2 y2 r sub lineto
  x2 r sub y2 r sub r 0 90 arc
  x r add y2 lineto
  x r add y2 r sub r 90 180 arc
  x y r add lineto
  x r add y r add r 180 270 arc
  closepath
}} def
gsave
0 {ARTBOARD} translate
1 -1 scale
{ps_color(DARK)}
{x1:.2f} {y1:.2f} {x2:.2f} {y2:.2f} {r:.2f} roundrect fill
{ps_polygon(LEFT_TAIL)}
{ps_color(WARM)}
{x1b:.2f} {y1b:.2f} {x2b:.2f} {y2b:.2f} {rb:.2f} roundrect fill
{ps_polygon(RIGHT_TAIL)}
{ps_color(WHITE)}
{ps_polygon(CHEVRON)}
grestore
showpage
%%EOF
"""


def draw_variant(
    size: int,
    *,
    background: str | None,
    left_fill: str,
    right_fill: str,
    chevron_fill: str,
    supersample: int = 4,
) -> Image.Image:
    render_size = size * supersample
    scale = render_size / ARTBOARD
    image = Image.new("RGBA", (render_size, render_size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)

    if background:
        draw.rectangle((0, 0, render_size, render_size), fill=hex_to_rgba(background))

    left = [value * scale for value in LEFT_RECT[:4]]
    right = [value * scale for value in RIGHT_RECT[:4]]
    radius = LEFT_RECT[4] * scale
    right_radius = RIGHT_RECT[4] * scale

    draw.rounded_rectangle(left, radius=radius, fill=hex_to_rgba(left_fill))
    draw.polygon([(x * scale, y * scale) for x, y in LEFT_TAIL], fill=hex_to_rgba(left_fill))

    draw.rounded_rectangle(right, radius=right_radius, fill=hex_to_rgba(right_fill))
    draw.polygon([(x * scale, y * scale) for x, y in RIGHT_TAIL], fill=hex_to_rgba(right_fill))

    draw.polygon([(x * scale, y * scale) for x, y in CHEVRON], fill=hex_to_rgba(chevron_fill))

    if supersample == 1:
        return image

    return image.resize((size, size), Image.Resampling.LANCZOS)


def write_text_assets() -> None:
    (BRANDING_DIR / "kotalk-logo-master.svg").write_text(
        svg_document(background=None, left_fill=DARK, right_fill=WARM, chevron_fill=WHITE),
        encoding="utf-8",
    )
    (BRANDING_DIR / "kotalk-logo-master.eps").write_text(eps_document(), encoding="utf-8")

    subprocess.run(
        [
            "gs",
            "-dBATCH",
            "-dNOPAUSE",
            "-dSAFER",
            "-sDEVICE=pdfwrite",
            f"-sOutputFile={BRANDING_DIR / 'kotalk-logo-master.pdf'}",
            str(BRANDING_DIR / "kotalk-logo-master.eps"),
        ],
        check=True,
        cwd=ROOT,
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
    )

    transparent_svg = svg_document(background=None, left_fill=DARK, right_fill=WARM, chevron_fill=WHITE)
    mono_svg = svg_document(background=None, left_fill=BLACK, right_fill=BLACK, chevron_fill=BLACK)
    inverse_svg = svg_document(background=NIGHT, left_fill=PAPER, right_fill=WARM, chevron_fill=NIGHT)

    (WEB_PUBLIC_DIR / "icon.svg").write_text(transparent_svg, encoding="utf-8")
    (WEB_PUBLIC_DIR / "vs-mark.svg").write_text(transparent_svg, encoding="utf-8")
    (WEB_PUBLIC_DIR / "mask-icon.svg").write_text(inverse_svg, encoding="utf-8")
    (WEB_PUBLIC_DIR / "apple-touch-icon.svg").write_text(transparent_svg, encoding="utf-8")


def write_png_assets() -> None:
    transparent_sizes = [1024, 512, 256, 192, 180, 128, 64, 32, 16]
    for size in transparent_sizes:
        transparent = draw_variant(
            size,
            background=None,
            left_fill=DARK,
            right_fill=WARM,
            chevron_fill=WHITE,
        )
        transparent.save(PNG_DIR / f"kotalk-transparent-{size}.png")

    draw_variant(
        1024,
        background=WHITE,
        left_fill=DARK,
        right_fill=WARM,
        chevron_fill=WHITE,
    ).save(PNG_DIR / "kotalk-white-1024.png")
    draw_variant(
        1024,
        background=None,
        left_fill=BLACK,
        right_fill=BLACK,
        chevron_fill=BLACK,
    ).save(PNG_DIR / "kotalk-mono-black-1024.png")
    draw_variant(
        1024,
        background=None,
        left_fill=WHITE,
        right_fill=WHITE,
        chevron_fill=WHITE,
    ).save(PNG_DIR / "kotalk-mono-white-1024.png")
    draw_variant(
        1024,
        background=NIGHT,
        left_fill=PAPER,
        right_fill=WARM,
        chevron_fill=NIGHT,
    ).save(PNG_DIR / "kotalk-inverse-1024.png")

    draw_variant(
        512,
        background=WHITE,
        left_fill=DARK,
        right_fill=WARM,
        chevron_fill=WHITE,
    ).save(WEB_PUBLIC_DIR / "icon-512.png")
    draw_variant(
        192,
        background=WHITE,
        left_fill=DARK,
        right_fill=WARM,
        chevron_fill=WHITE,
    ).save(WEB_PUBLIC_DIR / "icon-192.png")
    draw_variant(
        180,
        background=WHITE,
        left_fill=DARK,
        right_fill=WARM,
        chevron_fill=WHITE,
    ).save(WEB_PUBLIC_DIR / "apple-touch-icon.png")
    draw_variant(
        32,
        background=None,
        left_fill=DARK,
        right_fill=WARM,
        chevron_fill=WHITE,
    ).save(WEB_PUBLIC_DIR / "favicon-32x32.png")
    draw_variant(
        16,
        background=None,
        left_fill=DARK,
        right_fill=WARM,
        chevron_fill=WHITE,
    ).save(WEB_PUBLIC_DIR / "favicon-16x16.png")
    draw_variant(
        128,
        background=None,
        left_fill=DARK,
        right_fill=WARM,
        chevron_fill=WHITE,
    ).save(DESKTOP_ASSETS_DIR / "kotalk-mark-128.png")


def write_ico_assets() -> None:
    desktop_icon = draw_variant(
        256,
        background=None,
        left_fill=DARK,
        right_fill=WARM,
        chevron_fill=WHITE,
    )
    desktop_icon.save(
        ICO_DIR / "kotalk.ico",
        format="ICO",
        sizes=[(256, 256), (128, 128), (64, 64), (48, 48), (32, 32), (16, 16)],
    )
    desktop_icon.save(
        DESKTOP_ASSETS_DIR / "kotalk.ico",
        format="ICO",
        sizes=[(256, 256), (128, 128), (64, 64), (48, 48), (32, 32), (16, 16)],
    )

    favicon_icon = draw_variant(
        64,
        background=None,
        left_fill=DARK,
        right_fill=WARM,
        chevron_fill=WHITE,
        supersample=6,
    )
    favicon_icon.save(
        ICO_DIR / "favicon.ico",
        format="ICO",
        sizes=[(64, 64), (32, 32), (16, 16)],
    )
    favicon_icon.save(
        WEB_PUBLIC_DIR / "favicon.ico",
        format="ICO",
        sizes=[(64, 64), (32, 32), (16, 16)],
    )


def main() -> None:
    ensure_dirs()
    write_text_assets()
    write_png_assets()
    write_ico_assets()
    print("Generated KoTalk brand assets in branding/, src/PhysOn.Web/public/, and src/PhysOn.Desktop/Assets/.")


if __name__ == "__main__":
    main()
