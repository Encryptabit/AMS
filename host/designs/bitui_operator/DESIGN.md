# Design System Specification: The Command Surface

## 1. Overview & Creative North Star
### Creative North Star: "The Orchestrator’s Lens"
This design system is engineered for high-stakes, high-density operator environments where data is the primary protagonist. Moving away from the "airy" trends of consumer web apps, this system embraces **Organic Brutalism**—a philosophy of sharp precision, zero-radius corners, and maximum information throughput. 

We break the "standard template" look by utilizing intentional density. Instead of hiding data behind clicks, we surface it through a sophisticated hierarchy of tonal shifts. The aesthetic is "Instrument-Grade": it feels like a precision surgical tool or a high-end aviation console. It is unapologetically professional, silent when stable, and authoritative when demanding attention.

---

## 2. Colors & Surface Architecture
The palette is rooted in deep oceanic slates and nocturnal navies, providing a low-fatigue environment for long-duration monitoring.

### The "No-Line" Rule
Explicitly prohibit the use of 1px solid borders for structural sectioning. Boundaries must be defined solely through background color shifts or subtle tonal transitions. Use `surface-container-low` against `surface` to define a sidebar; use `surface-container-high` to define a focus area. 

### Surface Hierarchy & Nesting
Treat the UI as a series of machined layers. Depth is achieved by "stacking" container tiers:
- **Base Layer:** `surface` (#060e20)
- **Primary Layout Blocks:** `surface-container-low` (#06122d)
- **Interactive Modules:** `surface-container-high` (#031d4b)
- **Active Focus/Popovers:** `surface-bright` (#002867)

### Signature Textures & Soul
To prevent the UI from feeling "dead," use a subtle linear gradient on primary CTAs and header accents:
- **Action Gradient:** `primary` (#9fcaff) to `primary_container` (#00497e) at a 135° angle.
- **Glassmorphism:** For floating utility panels, use `surface_variant` at 60% opacity with a `20px` backdrop-blur.

---

## 3. Typography: The Micro-Grid
We use **Inter** for its exceptional legibility at small scales. In this system, typography is an information density tool, not just a decorative element.

- **Data-Heavy Display:** `label-sm` (11px) and `label-md` (12px) are your workhorses. Use `on_surface_variant` (#91aaeb) for metadata to keep the primary `on_surface` (#dee5ff) text readable.
- **The Hierarchy:** 
    - **Display-SM (36px):** Reserved for critical system KPIs.
    - **Title-SM (16px):** Section headers, bold weight.
    - **Body-SM (12px):** The standard for all data table entries and operator logs.
- **Intentional Asymmetry:** Align headers to the left with a massive `headline-lg` scale against a tiny `label-sm` timestamp to create an editorial, high-end feel.

---

## 4. Elevation & Tonal Layering
Traditional drop shadows have no place in a precision workstation. We use **Tonal Layering**.

- **The Layering Principle:** Depth is achieved by "stacking." A `surface-container-lowest` card placed on a `surface-container-low` background creates a "recessed" look.
- **Ambient Shadows:** Only for floating modals. Use a tinted shadow: `0px 8px 24px rgba(0, 0, 0, 0.5)` with a subtle glow of `surface_tint` at 4% opacity.
- **The "Ghost Border" Fallback:** If containment is visually necessary for accessibility, use a `1px` stroke of `outline_variant` (#2b4680) at **15% opacity**. Never use 100% opaque lines.

---

## 5. Components
All components feature a **0px border-radius** (Sharp-Edge) to maintain the "BitUI" aesthetic.

### Buttons & Controls
- **Primary Button:** Gradient fill (Primary to Primary-Container). No border. Label: `label-md` uppercase.
- **Segmented Controls:** A single `surface-container-highest` track with `primary` indicator blocks. Use `0px` rounding.
- **Double-Ended Range Sliders:** `outline-variant` track (thin 2px) with `primary` thumb blocks (8px x 12px rectangles).

### Data Tables (The Core)
- **Row Styling:** No dividers. Use a `surface-container-high` background on `:hover`.
- **Progress Bars:** Ultra-thin (4px). Background: `surface-variant`. Fill: `primary`.
- **Status Chips (Compact):** 
    - **Success:** `on_surface` text with a 2px left-border of Green.
    - **Running:** `on_surface` text with a 2px left-border of Yellow.
    - **Failed:** `error` (#ee7d77) text with a `error_container` (#7f2927) background.

### Input Fields
- **Compact Inputs:** `surface-container-lowest` background. Underline only (2px) using `outline` (#5b74b1). When focused, the underline shifts to `primary`.

---

## 6. Do's and Don'ts

### Do:
- **Do** prioritize information density. If there is empty space, consider if more telemetry could be surfaced.
- **Do** use `letter-spacing: 0.05em` for all `label-sm` text to ensure legibility on dark backgrounds.
- **Do** use monospaced numbers (Inter features tabular num glyphs) for all data tables to prevent "jumping" during updates.

### Don't:
- **Don't** use rounded corners. The `roundedness scale` is strictly `0px`.
- **Don't** use dividers or lines to separate list items. Use 4px or 8px of vertical whitespace or tonal shifts.
- **Don't** use bright "pure white." All "white" text should be mapped to `on_surface` (#dee5ff) to prevent eye strain in dark environments.
- **Don't** use standard "Drop Shadows." If it doesn't look like it's part of the machine, it doesn't belong.