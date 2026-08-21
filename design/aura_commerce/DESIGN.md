---
name: Aura Commerce
colors:
  surface: '#fbf9f8'
  surface-dim: '#dcd9d9'
  surface-bright: '#fbf9f8'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f6f3f2'
  surface-container: '#f0eded'
  surface-container-high: '#eae8e7'
  surface-container-highest: '#e4e2e1'
  on-surface: '#1b1c1c'
  on-surface-variant: '#44474d'
  inverse-surface: '#303030'
  inverse-on-surface: '#f3f0f0'
  outline: '#75777e'
  outline-variant: '#c5c6cd'
  surface-tint: '#515f78'
  primary: '#000000'
  on-primary: '#ffffff'
  primary-container: '#0d1c32'
  on-primary-container: '#76849f'
  inverse-primary: '#b9c7e4'
  secondary: '#006398'
  on-secondary: '#ffffff'
  secondary-container: '#6cbdfe'
  on-secondary-container: '#004b75'
  tertiary: '#000000'
  on-tertiary: '#ffffff'
  tertiary-container: '#001a41'
  on-tertiary-container: '#2180ff'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#d6e3ff'
  primary-fixed-dim: '#b9c7e4'
  on-primary-fixed: '#0d1c32'
  on-primary-fixed-variant: '#39475f'
  secondary-fixed: '#cde5ff'
  secondary-fixed-dim: '#94ccff'
  on-secondary-fixed: '#001d32'
  on-secondary-fixed-variant: '#004b74'
  tertiary-fixed: '#d8e2ff'
  tertiary-fixed-dim: '#adc7ff'
  on-tertiary-fixed: '#001a41'
  on-tertiary-fixed-variant: '#004493'
  background: '#fbf9f8'
  on-background: '#1b1c1c'
  surface-variant: '#e4e2e1'
typography:
  display-lg:
    fontFamily: Inter
    fontSize: 48px
    fontWeight: '700'
    lineHeight: '1.1'
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '600'
    lineHeight: '1.2'
  headline-lg-mobile:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: '1.2'
  headline-md:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: '1.3'
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: '1.6'
  body-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: '1.5'
  body-sm:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: '1.4'
  label-md:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '600'
    lineHeight: '1'
    letterSpacing: 0.05em
  price-lg:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '700'
    lineHeight: '1'
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  base: 8px
  container-max: 1280px
  gutter: 24px
  margin-mobile: 16px
  stack-sm: 8px
  stack-md: 16px
  stack-lg: 32px
  section-padding: 80px
---

## Brand & Style

The design system is rooted in **Minimalism** and **Modern Corporate** aesthetics, specifically tailored for a premium e-commerce experience. The brand personality is professional, reliable, and unobtrusive, allowing product imagery to take center stage. 

The visual language emphasizes clarity through generous whitespace, high-quality typography, and a "soft-layering" approach to depth. By combining a deep navy foundation with bright, functional accents, the system evokes a sense of established trust and technological sophistication. The interface should feel "light" and "airy," utilizing subtle transitions and crisp borders to define the user journey from discovery to checkout.

## Colors

The palette is anchored by **Dark Navy (#0A192F)**, used for primary navigation and core branding elements to establish authority. **Bright Blue (#007BFF)** serves as the high-visibility accent color reserved strictly for call-to-action (CTA) buttons, links, and prices to drive conversion.

- **Primary:** Use for headers, footers, and primary text headings.
- **Secondary:** Use for secondary buttons, icon backgrounds, or subtle UI highlights.
- **Background:** A very light grey (#F8F9FA) ensures the white surface cards "pop" with clear definition.
- **Status Tokens:** Specifically mapped for e-commerce order management. Use low-saturation backgrounds with high-saturation text for badges (e.g., a light green background with dark green text for 'Delivered').

## Typography

This design system utilizes **Inter** exclusively to ensure a systematic, utilitarian, and modern feel across all touchpoints. 

- **Scale:** A tight typographic scale ensures hierarchy without overwhelming the content. 
- **Headlines:** Use heavy weights (600-700) with slight negative letter-spacing for a premium, "tight" editorial look.
- **Prices:** Treated as a distinct typographic role. They should be bold and use the accent color to ensure they are the first thing a user sees on a product card.
- **Readability:** Body copy uses a generous 1.5-1.6 line height to maintain legibility in product descriptions.

## Layout & Spacing

The layout follows a **12-column fluid grid** system built on an 8px base unit, aligning perfectly with Bootstrap 5's grid logic. 

- **Desktop:** 1280px max-width container with 24px gutters. Use 80px - 100px vertical padding between major homepage sections.
- **Mobile:** Transition to a single-column layout with 16px side margins. 
- **Product Grids:** Use a 2-column layout on mobile and a 4-column layout on desktop to maximize density without sacrificing clarity.
- **Rhythm:** Use the `stack` variables for vertical spacing between elements (e.g., 8px between a title and its category label, 16px between a title and a price).

## Elevation & Depth

Visual hierarchy is achieved through **Tonal Layers** and **Ambient Shadows**. 

1.  **Level 0 (Base):** The Background color (#F8F9FA).
2.  **Level 1 (Surface):** White cards (#FFFFFF). These should have a subtle, wide-spread shadow: `0 4px 20px rgba(0, 0, 0, 0.05)`.
3.  **Level 2 (Interaction):** On hover, cards should lift slightly using a more pronounced shadow: `0 8px 30px rgba(0, 0, 0, 0.08)` and a subtle Y-axis translation (-4px).
4.  **Level 3 (Overlays):** Modals and dropdowns use the most depth, featuring a `0 12px 40px rgba(0, 0, 0, 0.12)` shadow to separate them from the shopping interface.

Avoid heavy black shadows; always use low-opacity alpha values to maintain the "clean" aesthetic.

## Shapes

The design system uses **Rounded (Option 2)** geometry to evoke a friendly yet professional feel.

- **Cards:** Use `rounded-lg` (16px) for main product cards and containers to create a soft, premium look.
- **Buttons & Inputs:** Use a standard `rounded` (8px) for a consistent, structured appearance.
- **Badges:** Use `rounded-pill` for status indicators (Pending, Shipped, etc.) to distinguish them from clickable buttons.
- **Images:** Product images within cards should inherit the card's top-border radius (16px) for a seamless, integrated appearance.

## Components

### Buttons
- **Primary:** Background #007BFF, Text #FFFFFF. Bold, 8px border radius.
- **Secondary:** Outline #0A192F, Text #0A192F. Minimalist approach for "Add to Wishlist" or "View Details."

### Cards (Product)
- **Structure:** Image at top, followed by 16px padding. Title (Headline-md), Price (Price-lg), and a "Quick Add" secondary button. 
- **Elevation:** Level 1 shadow by default, Level 2 on hover.

### Form Fields
- **Style:** Background #FFFFFF, Border 1px solid #DEE2E6. On focus, border changes to #64B5F6 with a subtle glow (0 0 0 4px rgba(100, 181, 246, 0.25)).

### Status Badges
- **General:** Small, bold, uppercase labels with a 12px font size.
- **Pending:** Grey background, dark grey text.
- **Processing:** Soft blue background, primary navy text.
- **Delivered:** Pale green background, dark green text.
- **Cancelled:** Pale red background, dark red text.

### Navigation (Header)
- **Style:** Fixed white surface with a bottom border (1px solid #EDEDED). Deep navy (#0A192F) links with an active state indicated by a 2px bottom bar in Bright Blue (#007BFF).