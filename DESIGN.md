---
version: alpha
name: Aura Commerce
colors:
  background: "#fbf9f8"
  surface: "#ffffff"
  surfaceSoft: "#f0eded"
  text: "#1b1c1c"
  muted: "#5f6368"
  primary: "#0a192f"
  accent: "#0062cc"
  success: "#16794b"
  danger: "#ba1a1a"
typography:
  display:
    fontFamily: "Inter, Noto Kufi Arabic, system-ui, sans-serif"
    fontWeight: "700"
  body:
    fontFamily: "Inter, Noto Kufi Arabic, system-ui, sans-serif"
    fontSize: "16px"
    lineHeight: "1.5"
rounded:
  sm: "4px"
  DEFAULT: "8px"
  card: "16px"
  pill: "9999px"
spacing:
  base: "8px"
  gutter: "24px"
  section: "80px"
components:
  button:
    backgroundColor: "{colors.accent}"
    textColor: "#ffffff"
    rounded: "{rounded.DEFAULT}"
  card:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.text}"
    rounded: "{rounded.card}"
  input:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.text}"
    rounded: "{rounded.DEFAULT}"
  mutedText:
    textColor: "{colors.muted}"
  softSurface:
    backgroundColor: "{colors.surfaceSoft}"
  successStatus:
    textColor: "{colors.success}"
  dangerStatus:
    textColor: "{colors.danger}"
---

## Overview

Aura Commerce is a product-first storefront and operations workspace. It should feel like a calm, well-run showroom: product photography and operational facts lead, while chrome stays restrained. The public store is spacious; admin screens are denser without changing the brand. Avoid marketplace clutter, neon dashboard effects, glassmorphism, and decorative motion.

## Colors

Dark navy establishes trust in navigation and headings. Bright blue is reserved for primary actions, links, and selected states. Status colors always pair text or an icon with color. Dark mode preserves the same semantic hierarchy through the runtime variables in `OnlineStore/wwwroot/css/site.css`, which is the canonical token implementation.

## Typography

Inter is the Latin display/body face and Noto Kufi Arabic is the Arabic fallback. Prices and operational totals use strong weight; body copy remains regular. Arabic layouts must preserve readable line height and RTL flow.

## Layout

Use the existing Bootstrap grid, 1280px content container, 24px desktop gutters, 16px mobile margins, and an 8px spacing rhythm. Public product grids favor imagery. The customer header stays collapsed through extra-large widths and expands only when the full navigation, search, and account controls fit without horizontal overflow. Profile content uses the full-width form and a two-card summary band at medium and extra-large widths, then adopts the established 8/4 form-and-sidebar split at extra-extra-large widths. The profile content begins 48px below its hero so the account workspace remains compact without crowding the heading. Admin page headers lead with the page title and actions, followed immediately by the shared admin navigation; list content starts below the header. Admin lists favor bounded responsive tables with URL-restorable filtering and pagination.

## Elevation & Depth

Cards use quiet ambient shadows and thin borders. Hover elevation is limited to navigable product cards; tables and forms remain stable.

## Shapes

Cards use 16px corners, controls 8px, and status badges pill geometry. Rounded shapes signal grouping, not decoration.

## Components

The shared Razor layout, `site.css`, and `site.js` own navigation, theme, language, focus, notification, and motion behavior. Native selects and dates are intentional because platform-owned popup behavior is accepted. All application forms own validation with `novalidate` and visible inline errors. External sign-in uses full-width, provider-named controls above a restrained email divider; unavailable providers stay visible but clearly disabled with a short setup-status explanation.

## Do's and Don'ts

- Do keep save/cancel outcomes and toast wording consistent.
- Do preserve table actions and labels at narrow widths through visible overflow.
- Do use semantic links and buttons with visible focus.
- Don't use browser alert, confirm, or prompt.
- Don't hide unavailable, destructive, or financial behavior behind ambiguous icons.
