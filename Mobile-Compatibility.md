# AMS Workstation — Mobile Compatibility Plan

## Current State

The Workstation is a Blazor Server app (`host/Ams.Workstation.Server/`) using Bit.BlazorUI components. It has minimal responsive CSS (a 960px breakpoint that stacks the layout vertically and hides the inspector panel) but is effectively unusable on phones because:

- The **header controls** (stage nav, chapter dropdown, workspace dropdown, directory input) overflow horizontally — users can't select chapters or switch workspaces.
- The **Modules sidebar** always renders as a full-width list, consuming screen real estate.
- All interaction relies on **keyboard shortcuts** (`H/L` nav, `E` export, `D` reviewed, `Space` play, etc.) with zero touch/gesture equivalents.
- The **CRX export modal** has `min-width: 600px` and a two-column grid that doesn't collapse.
- There is no way to **export from the Playback view** (only Errors view has the export trigger).
- There is no way to **export multiple sentences** to CRX at once.
- Export breaks on **0-second sentences** — no way to manually set the time frame.
- The **WaveformPlayer** controls (zoom slider, speed slider) are cramped on narrow screens.

The earlier `tools/validation-viewer` (vanilla JS) already solved many of these problems: swipe gestures, auto-hiding header, bottom action bar, full-screen modals, chapter-complete marking, and multi-select export. This plan ports those patterns into the Blazor Workstation.

---

## Ordered Implementation Phases

### Phase 1 — Responsive Header & Navigation

**Goal:** Make the top nav bar fully usable on any screen width.

**Files:**
- `Components/Layout/HeaderControls.razor` / `.razor.css`
- `Components/Layout/MainLayout.razor` / `.razor.css`

**Tasks:**

1. **Add mobile breakpoint CSS to HeaderControls** (`max-width: 768px`)
   - Hide the directory text field and "Set" / "Reset Metadata" buttons behind a hamburger/overflow menu icon.
   - Stack the stage nav buttons (Prep / Proof / Polish) as compact icon-pills or a single dropdown.
   - Keep the chapter dropdown and workspace dropdown visible but set `width: auto; min-width: 0; flex-shrink: 1` so they compress gracefully.

2. **Introduce a mobile overflow menu**
   - Add a `BitButton` with a kebab/hamburger icon, visible only at `≤768px`.
   - On tap, open a `BitPanel` (drawer) or `BitCallout` containing: directory input, "Set" button, "Reset Metadata", and any future settings.

3. **Ensure chapter & workspace dropdowns are always reachable**
   - On narrow screens, let them wrap to a second row if needed (`flex-wrap: wrap` on `.header-controls`).
   - Increase touch target height to 44px minimum.

4. **Viewport meta tag**
   - Confirm `<meta name="viewport" content="width=device-width, initial-scale=1">` exists in `App.razor`. Add if missing.

---

### Phase 2 — Collapsible Module Rail

**Goal:** Modules list should not consume permanent screen space on mobile.

**Files:**
- `Components/Layout/StageModuleRail.razor` / `.razor.css`
- `Components/Layout/MainLayout.razor` / `.razor.css`

**Tasks:**

1. **Hide the sidebar by default on mobile** (`≤768px`)
   - `display: none` on `.workstation-sidebar` at the mobile breakpoint.

2. **Add a "Modules" toggle button** visible only on mobile
   - Place a floating button (bottom-left or top-left) or integrate into the header overflow menu.
   - On tap, slide the module rail in as a `BitPanel` drawer (overlay, left edge) or a bottom sheet.
   - Auto-close after selecting a module (navigation occurs).

3. **Alternatively: bottom tab bar** (validation-viewer pattern)
   - If the current stage has ≤5 modules, render them as a fixed bottom tab bar on mobile (icon + short label).
   - This mirrors the validation-viewer's mobile action bar and keeps one-tap access.

---

### Phase 3 — Touch Gesture Layer

**Goal:** Provide gesture equivalents for every keyboard shortcut.

**Files:**
- New: `wwwroot/js/touch-gestures.js` (ES module)
- `Components/Pages/Proof/ChapterReview.razor` / `.razor.cs`
- `Components/Shared/WaveformPlayer.razor`

**Keybinding → Gesture Mapping:**

| Keyboard | Gesture | Target Element |
|----------|---------|----------------|
| `←` / `H` (prev item) | Swipe right on sentence list | `.sentence-row` / `.compact-sentence` |
| `→` / `L` (next item) | Swipe left on sentence list | `.sentence-row` / `.compact-sentence` |
| `Space` (play/pause) | Tap waveform | `.waveform-container` |
| `E` (open CRX export) | Swipe right on sentence | Individual sentence element |
| `D` (toggle reviewed) | Long-press on sentence | Individual sentence element |
| `I` (ignore error) | Swipe left on sentence (errors view) | Error card / sentence row |
| `Alt+←/→` (chapter nav) | Swipe left/right on header chapter area | `.header-controls` chapter region |
| `Ctrl+←/→` (cross-nav) | Bottom tab bar tap | Module/view tabs |

**Tasks:**

1. **Create `touch-gestures.js`** module
   - Reuse the validation-viewer's touch detection pattern: `touchstart`/`touchend` with delta thresholds (50px min swipe, must be more horizontal than vertical, < 500ms duration).
   - Export `init(dotNetRef)` and `dispose()` functions.
   - On detected gesture, call the corresponding `[JSInvokable]` method on the Blazor component (same handlers the keyboard module calls: `OnNavigateItem`, `OnOpenCrx`, `OnToggleReviewed`, `OnIgnoreError`, etc.).

2. **Wire up in `ChapterReview.razor`**
   - `OnAfterRenderAsync`: call `touch-gestures.js init()` with the `DotNetObjectReference`.
   - `Dispose`: call `dispose()`.

3. **Add visual gesture hints**
   - On first mobile visit (localStorage flag), show a brief overlay: "Swipe → to export · Swipe ← to navigate · Tap waveform to play".
   - Dismissable, with "Don't show again" option.

4. **Prevent iOS double-tap zoom and pull-to-refresh** inside the app shell
   - Add `touch-action: manipulation` on interactive containers.

---

### Phase 4 — Mobile-Friendly CRX Export Modal

**Goal:** CRX modal usable on phones; support custom time frame and multi-sentence export.

**Files:**
- `Components/Shared/CrxModal.razor` / inline CSS

**Tasks:**

1. **Responsive modal layout**
   - Remove `min-width: 600px`.
   - At `≤768px`: modal goes full-screen (`width: 100vw; height: 100vh; border-radius: 0`), matching the validation-viewer pattern.
   - Collapse the two-column grid to single column on mobile.
   - Make the audio preview waveform height smaller on mobile (60px instead of 128px).
   - Sticky header (title + close button) and sticky footer (action buttons).

2. **Add editable start/end time fields**
   - Replace the read-only time display with two editable `BitTextField` inputs (`mm:ss.mmm` format).
   - Pre-populate from the sentence boundaries.
   - Validate: start < end, both within chapter duration.
   - When the user edits times, update the audio preview URL and waveform region.
   - This fixes the 0-second sentence export bug — user can widen the window manually.

3. **Add a "use region" mode**
   - Allow users to drag the waveform region handles to visually set the export range.
   - Sync region changes back to the time text fields.
   - The WaveformPlayer already supports `AddEditableRegion` and `OnRegionUpdated` — wire these into the modal.

4. **Touch-friendly controls**
   - Increase checkbox hit areas to 44px.
   - Make the padding slider thumb larger on mobile.
   - Ensure the comments textarea is at least 3 rows and doesn't trigger iOS zoom (set `font-size: 16px`).

---

### Phase 5 — Multi-Sentence CRX Export

**Goal:** Select multiple sentences and export them as a batch to CRX.

**Files:**
- `Components/Pages/Proof/ChapterReview.razor` / `.razor.cs`
- `Components/Shared/CrxModal.razor`
- `Services/CrxService.cs`
- New: selection state management (in `ChapterReview`)

**Tasks:**

1. **Add multi-select state to `ChapterReview`**
   - `HashSet<string> _selectedSentenceIds` — tracks selected sentences.
   - Toggle selection: long-press (mobile) or Ctrl+click (desktop) on a sentence row.
   - Visual indicator: highlight bar or checkbox overlay on selected sentences.
   - Show a floating "X selected" badge when selection is active.

2. **Batch export action**
   - When selection is active and user triggers export (swipe-right or `E` key):
     - Compute time range as `min(startTimes) → max(endTimes)` across all selected sentences.
     - Open CRX modal with the merged range.
     - Auto-generate combined comments from all selected sentences' diffs.
   - "Clear selection" button to deselect all.

3. **Update `CrxService` if needed**
   - Ensure the submit request can carry multiple sentence references.
   - Or submit one CRX entry with the merged time range and combined comments (simpler).

4. **Playback view support**
   - Multi-select must also work in Playback view, not just Errors view.
   - Swipe-left on a sentence in playback toggles selection (matching validation-viewer pattern).

---

### Phase 6 — Export from Playback View

**Goal:** Users can trigger CRX export directly from the Playback view.

**Files:**
- `Components/Pages/Proof/ChapterReview.razor` / `.razor.cs`
- `wwwroot/js/keyboard-shortcuts.js`

**Tasks:**

1. **Enable `E` key in Playback view**
   - Currently `OnOpenCrx()` resolves the sentence from the errors list. Update to also resolve from `_selectedSentenceId` or `_playbackNavigationIndex` when in Playback view.
   - Look up the `SentenceReport` from the chapter data by sentence ID.

2. **Add an "Export" button to the Playback view sentence row**
   - Small icon button (export/share icon) on each sentence row, visible on hover (desktop) and always visible (mobile).
   - On tap, opens CRX modal for that sentence.

3. **Wire gesture**
   - Swipe-right on a sentence in Playback view → opens CRX modal (same as Phase 3 mapping).

---

### Phase 7 — Chapter Completion / Mark as Reviewed

**Goal:** Port the validation-viewer's "mark chapter complete" workflow.

**Files:**
- `Components/Pages/Proof/ChapterReview.razor` / `.razor.cs`
- `Services/` (new or existing reviewed-status service)
- `Components/Layout/HeaderControls.razor`

**Tasks:**

1. **Reviewed status persistence**
   - Store reviewed state per chapter in workspace metadata (JSON file, like validation-viewer's `reviewed-status.json`).
   - Service: `IChapterReviewStatusService` with `MarkReviewed(chapter, bool)` and `IsReviewed(chapter)`.

2. **"Mark as Reviewed" action**
   - Already exists as a tab/button in ChapterReview (`Mark as Reviewed` button). Ensure it actually persists.
   - On mobile: accessible from the bottom action bar or via long-press on the chapter dropdown entry.
   - `D` key already calls `OnToggleReviewed()` — ensure this persists to disk.

3. **Visual indicator in chapter list**
   - In the header chapter dropdown, show a checkmark or dimmed style for reviewed chapters.
   - Optionally auto-advance to the next unreviewed chapter after marking complete.

---

### Phase 8 — Mobile Playback Controls

**Goal:** WaveformPlayer controls are comfortable on small screens.

**Files:**
- `Components/Shared/WaveformPlayer.razor` / `.razor.css`
- `wwwroot/js/waveform-interop.js`

**Tasks:**

1. **Responsive player height**
   - At `≤768px`, reduce default waveform height to 80px (from 128px).
   - Disable or hide the zoom slider on mobile (zoom is less useful with touch; pinch-to-zoom can be added later).

2. **Larger transport buttons**
   - Play/pause, skip-start, skip-end buttons: minimum 44×44px touch targets.
   - Space them with adequate gap (12px+).

3. **Speed control**
   - Replace the thin slider with a tap-to-cycle speed button on mobile (1x → 1.25x → 1.5x → 2x → 0.75x → 1x).
   - Show current speed as text inside/beside the button.

4. **Time display**
   - Ensure `mm:ss / mm:ss` doesn't truncate. Use `font-variant-numeric: tabular-nums` for stable width.

5. **Tap-to-seek**
   - Verify WaveSurfer's built-in click-to-seek works on touch. If not, add a `touchend` handler on the waveform container that calls `seekTo()`.

---

### Phase 9 — Bottom Action Bar (Mobile)

**Goal:** Persistent mobile action bar for quick access to key actions.

**Files:**
- `Components/Layout/MainLayout.razor` / `.razor.css`
- New: `Components/Layout/MobileActionBar.razor`

**Tasks:**

1. **Create `MobileActionBar` component**
   - Fixed to bottom of viewport, visible only at `≤768px`.
   - Contains 4-5 icon buttons depending on context:
     - **Errors** — switch to errors view
     - **Playback** — switch to playback view
     - **Export** — trigger CRX export for current/selected sentence(s)
     - **Reviewed** — toggle chapter reviewed
     - **Modules** — open module drawer (from Phase 2)

2. **Context-aware buttons**
   - Highlight the active view button.
   - Show badge on "Export" when multi-select is active (count).
   - Disable "Export" when no sentence is focused.

3. **Safe area padding**
   - Account for iOS safe area (`env(safe-area-inset-bottom)`) so the bar doesn't overlap the home indicator.

4. **Body padding**
   - Add `padding-bottom` to main content area equal to action bar height so content isn't hidden behind it.

---

### Phase 10 — Auto-Hiding Mobile Header

**Goal:** Maximize screen real estate during playback scrolling.

**Files:**
- `Components/Layout/MainLayout.razor` / `.razor.css`
- New: `wwwroot/js/mobile-header.js` (or add to existing interop)

**Tasks:**

1. **Scroll-direction detection** (port from validation-viewer)
   - On scroll down > threshold (60px): slide header up off-screen (`transform: translateY(-100%)`).
   - On scroll up: slide header back.
   - Use `requestAnimationFrame` for smooth 60fps animation.
   - Disable auto-hide when a dropdown is open.

2. **CSS transitions**
   - `transition: transform 0.3s ease` on the header.
   - Adjust main content top padding dynamically when header is hidden.

---

### Phase 11 — Polish & Edge Cases

**Goal:** Final sweep for a complete mobile experience.

**Tasks:**

1. **iOS input zoom prevention**
   - Set `font-size: 16px` on all `<input>`, `<textarea>`, and `<select>` elements at mobile breakpoints.

2. **Touch-action CSS**
   - Apply `touch-action: manipulation` globally to prevent 300ms tap delay.
   - Apply `touch-action: pan-y` on horizontal-swipe containers to allow vertical scroll but capture horizontal gestures.

3. **Reconnect modal mobile styling**
   - Ensure the Blazor reconnect modal (`ReconnectModal.razor`) is centered and readable on small screens.

4. **Landscape orientation**
   - Test and ensure landscape mode on phones works — the waveform should expand horizontally, action bar stays visible.

5. **Performance**
   - Use passive touch event listeners where possible.
   - Debounce scroll handlers.
   - Lazy-load waveform peaks on mobile to reduce initial load time.

6. **Accessibility**
   - Ensure all gesture-based actions have a visible button alternative.
   - ARIA labels on icon-only buttons in the action bar.

---

## Implementation Order & Dependencies

```
Phase 1  ─── Responsive Header ──────────────────────┐
Phase 2  ─── Collapsible Modules ─────────────────────┤
Phase 4  ─── Responsive CRX Modal ───────────────────┤
Phase 8  ─── Mobile Playback Controls ────────────────┤── Can run in parallel
Phase 10 ─── Auto-Hiding Header ──────────────────────┘
                        │
Phase 9  ─── Bottom Action Bar ──── (depends on 1, 2)
Phase 3  ─── Touch Gesture Layer ── (depends on 9 for action bar context)
                        │
Phase 5  ─── Multi-Sentence Export ─ (depends on 3, 4)
Phase 6  ─── Export from Playback ── (depends on 4, 5)
Phase 7  ─── Mark as Reviewed ────── (depends on 9)
                        │
Phase 11 ─── Polish & Edge Cases ─── (depends on all above)
```

## Key Reference: Validation-Viewer Patterns to Port

| Validation-Viewer Feature | Location in `app.js` | Workstation Target |
|---------------------------|---------------------|--------------------|
| Swipe left/right gestures | Lines 688-751 | `touch-gestures.js` (Phase 3) |
| Auto-hiding header | Lines 2530-2571 | `mobile-header.js` (Phase 10) |
| Bottom action bar | `styles.css` Lines 1572-1842 | `MobileActionBar.razor` (Phase 9) |
| Full-screen modal on mobile | `styles.css` Lines 1716-1789 | CRX modal CSS (Phase 4) |
| Mark chapter reviewed | `app.js` Lines 300-327 | Review service (Phase 7) |
| Multi-select sentences | `app.js` Lines 836-888 | Selection state (Phase 5) |
| Tap-to-seek on sentences | `app.js` Lines 714-730 | Touch gestures (Phase 3) |
| Toast notifications | `app.js` Lines 2157-2179 | Already exists (Bit.BlazorUI) |
| 16px font to prevent zoom | `styles.css` Line 1740 | Global mobile CSS (Phase 11) |
