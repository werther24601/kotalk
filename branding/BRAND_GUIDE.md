# KoTalk Brand Guide

## Overview

This package turns the approved raster logo concept into a reusable production asset set for KoTalk.

- Reference intent: warm, calm, modern messenger mark
- Primary use: app icon, favicon, PWA icon, Windows icon, docs, release pages
- Master source: `branding/kotalk-logo-master.svg`

## File Inventory

### Master vectors

- `branding/kotalk-logo-master.svg`
- `branding/kotalk-logo-master.pdf`
- `branding/kotalk-logo-master.eps`

### PNG exports

- Transparent: `branding/png/kotalk-transparent-{1024|512|256|192|180|128|64|32|16}.png`
- White background: `branding/png/kotalk-white-1024.png`
- Mono black: `branding/png/kotalk-mono-black-1024.png`
- Mono white: `branding/png/kotalk-mono-white-1024.png`
- Inverse for dark surfaces: `branding/png/kotalk-inverse-1024.png`

### ICO exports

- Windows app icon: `branding/ico/kotalk.ico`
- Favicon bundle: `branding/ico/favicon.ico`

### Applied runtime assets

- Web app icons: `src/PhysOn.Web/public/`
- Desktop app icons: `src/PhysOn.Desktop/Assets/`

## Color System

### Core colors

- Ink: `#394350`
- Warm accent: `#F05B2B`
- Separator white: `#FFFFFF`

### Supporting backgrounds

- Paper: `#F7F3EE`
- Night: `#141922`

### Recommended backgrounds

- White: `#FFFFFF`
- Warm paper: `#F7F3EE`
- Dark surface: `#141922`

## Safe Area

- Artboard: `1024 x 1024`
- Recommended clear area from the artboard edge: `128px`
- Do not scale the visible mark so large that any speech bubble or the center chevron approaches the outer 12% of the square

## Minimum Use Size

- Preferred minimum digital size: `24px`
- Favicon minimum: `16px`
- When rendering below `24px`, use the packaged ICO or PNG exports instead of re-rasterizing from screenshots

## Usage Rules

- Keep the mark square, centered, and unrotated
- Use the transparent master for docs and UI surfaces when the background is already controlled
- Use the white background exports for launcher and touch icons
- Use the mono variants only where a single-color system is required

## Do Not

- Stretch or rotate the mark
- Add shadows, glow, gradients, glass, or texture
- Change the orange or dark brand colors arbitrarily
- Add pattern backgrounds behind the mark
- Recreate the logo from screenshots when the packaged vectors are available

## Regeneration

Run:

```bash
python scripts/branding/generate_kotalk_brand_assets.py
```

This regenerates the committed vector, PNG, ICO, and app-facing icon assets from the scripted master geometry.
