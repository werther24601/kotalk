#!/usr/bin/env python3
from __future__ import annotations

from pathlib import Path

from PIL import Image


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "branding" / "png" / "kotalk-transparent-1024.png"
ANDROID_RES = ROOT / "src" / "PhysOn.Mobile.Android" / "Resources"

BACKGROUND = (247, 243, 238, 255)
ICON_SIZES = {
    "mipmap-mdpi": 48,
    "mipmap-hdpi": 72,
    "mipmap-xhdpi": 96,
    "mipmap-xxhdpi": 144,
    "mipmap-xxxhdpi": 192,
}


def resize_logo(size: int, scale: float) -> Image.Image:
    source = Image.open(SOURCE).convert("RGBA")
    logo_size = max(8, round(size * scale))
    logo = source.resize((logo_size, logo_size), Image.Resampling.LANCZOS)

    canvas = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    offset = ((size - logo_size) // 2, (size - logo_size) // 2)
    canvas.alpha_composite(logo, offset)
    return canvas


def render_assets() -> None:
    for folder, size in ICON_SIZES.items():
        directory = ANDROID_RES / folder
        directory.mkdir(parents=True, exist_ok=True)

        background = Image.new("RGBA", (size, size), BACKGROUND)
        foreground = resize_logo(size, 0.74)
        full_icon = background.copy()
        full_icon.alpha_composite(resize_logo(size, 0.64))

        background.save(directory / "appicon_background.png")
        foreground.save(directory / "appicon_foreground.png")
        full_icon.save(directory / "appicon.png")


if __name__ == "__main__":
    render_assets()
