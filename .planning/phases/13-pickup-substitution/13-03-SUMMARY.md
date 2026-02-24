---
phase: 13-pickup-substitution
plan: 03
subsystem: ui
tags: [blazor, pickup-substitution, waveform, flippers, three-column, pipeline, mini-waveform]

# Dependency graph
requires:
  - phase: 13-pickup-substitution
    provides: "CrossChapterPickupMatch, PickupBoxState, drawMiniWaveform, waveform-data API (from 13-02)"
  - phase: 12-polish-area-foundation
    provides: "StagingQueueService, PolishService, PickupMatchingService, WaveformPlayer, PickupImporter"
provides:
  - "PickupSubstitution.razor single-page workflow at /polish/pickup"
  - "PickupBox.razor reusable card component for three-column pipeline"
  - "Flipper navigation between CRX-having chapters"
  - "Three-column pipeline layout (Matches | Staged | Committed)"
affects: [13-04, 13-05, 13-06, 13-07]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Single-page pickup workflow with flipper navigation between filtered chapters"
    - "Three-column pipeline layout (Match -> Stage -> Commit) using BitGrid 4/4/4"
    - "PickupBox component with canvas mini waveform and state-dependent actions"

key-files:
  created:
    - host/Ams.Workstation.Server/Components/Pages/Polish/PickupSubstitution.razor
    - host/Ams.Workstation.Server/Components/Shared/PickupBox.razor
  modified: []

key-decisions:
  - "Route set to /polish/pickup to avoid duplicate route conflict with existing Index.razor at /polish"
  - "CRX matching logic duplicated from ChapterPolish to keep components independent (can be extracted to shared service later)"
  - "Mini waveform uses JS module import per render; module is disposed after draw for memory efficiency"

patterns-established:
  - "Flipper navigation: filter chapters by CRX entries, navigate by index, reload chapter state on each flip"
  - "Column state derivation: Matches = cross-chapter pickups minus staged/applied; Staged = status==Staged; Committed = status==Applied"
  - "PickupBox reuse: same component renders differently per PickupBoxState enum (Matched/Staged/Committed)"

requirements-completed: [PS-LAYOUT, PS-NAV, PS-MATCH, PS-PIPELINE]

# Metrics
duration: 5min
completed: 2026-02-24
---

# Phase 13 Plan 03: Pickup Substitution Page & PickupBox Component Summary

**Single-page pickup substitution workflow with flipper-navigated chapter waveform, three-column pipeline layout, and reusable PickupBox card component with mini waveform thumbnails**

## Performance

- **Duration:** 5 min
- **Started:** 2026-02-24T19:14:22Z
- **Completed:** 2026-02-24T19:19:29Z
- **Tasks:** 2
- **Files created:** 2

## Accomplishments

- Created PickupSubstitution.razor at /polish/pickup with header bar (pickup session + roomtone file selectors), breadcrumb trail, chapter flippers, full waveform player, and three-column pipeline layout
- Created PickupBox.razor reusable component rendering pickup cards with canvas-based mini waveform thumbnails, confidence badges, matched text, and state-dependent action buttons (Stage/Unstage/Audition/Revert)
- Flipper navigation filters chapters to only those with CRX entries and loads chapter-specific flagged sentences and staging queue on each navigation

## Task Commits

Each task was committed atomically:

1. **Task 1: PickupSubstitution page -- header, breadcrumbs, flippers, waveform** - `698f42f` (feat)
2. **Task 2: PickupBox component with mini waveform and confidence display** - `84b75bc` (feat)

## Files Created/Modified

- `host/Ams.Workstation.Server/Components/Pages/Polish/PickupSubstitution.razor` - Single-page pickup substitution workflow with header, breadcrumbs, flippers, waveform, three-column layout
- `host/Ams.Workstation.Server/Components/Shared/PickupBox.razor` - Reusable pickup box card with mini waveform canvas, confidence badge, matched text, state-dependent actions, drag support

## Decisions Made

- **Route choice:** Used `/polish/pickup` instead of `/polish` to avoid duplicate route conflict with existing Index.razor. Plan specified `/polish` with note to resolve conflict in plan 13-07. Using a distinct route avoids ambiguity and runtime issues while the old Index.razor still serves the chapter list.
- **CRX matching logic placement:** Duplicated ChapterMatches/ResolveCrxTargets logic from ChapterPolish.razor into PickupSubstitution.razor to keep components independent. This can be extracted to a shared service in a later plan if needed.
- **Mini waveform module lifecycle:** Each PickupBox imports the JS module, draws the mini waveform, and disposes the module reference immediately. This avoids holding long-lived JS references for potentially many PickupBox instances on screen.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Used /polish/pickup route instead of /polish**
- **Found during:** Task 1 (PickupSubstitution page creation)
- **Issue:** Plan specified `@page "/polish"` which duplicates the existing Index.razor route, creating runtime ambiguity
- **Fix:** Used `@page "/polish/pickup"` as a distinct route
- **Files modified:** host/Ams.Workstation.Server/Components/Pages/Polish/PickupSubstitution.razor
- **Verification:** Build succeeds, no route conflicts
- **Committed in:** 698f42f (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Minor routing adjustment to avoid duplicate route ambiguity. Plan 13-07 will finalize the route structure.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- PickupSubstitution page ready for plan 13-04 (drag-and-drop wiring between columns)
- PickupBox component ready for plans 13-05/13-06 (matching UI and staging/commit UI enhancements)
- Three-column pipeline layout provides the structural foundation for the entire pickup workflow
- Flipper navigation between CRX chapters is operational

## Self-Check: PASSED

- All 2 created files exist on disk
- Both task commits verified (698f42f, 84b75bc)
- Key content verified: /polish/pickup route, PickupBoxState, loadAndDrawMiniWaveform, ChapterCrxInfo
- PickupSubstitution.razor: 726 lines (min 200), PickupBox.razor: 200 lines (min 50)
- Full solution Release build: 0 errors

---
*Phase: 13-pickup-substitution*
*Completed: 2026-02-24*
