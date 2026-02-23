---
phase: 12-polish-area-foundation
plan: 05
subsystem: ui
tags: [blazor, razor, bitui, polish, waveform, staging-queue, pickup-importer]

# Dependency graph
requires:
  - phase: 12-03
    provides: "PickupMatchingService and PolishService for import/stage/apply workflow"
  - phase: 12-04
    provides: "WaveformPlayer.AddEditableRegion for draggable boundary editing"
  - phase: 12-02
    provides: "StagingQueueService for non-destructive replacement queue"
provides:
  - "Polish landing page with chapter list and CRX error counts"
  - "ChapterPolish page with waveform, two-column layout (importer + queue)"
  - "PickupImporter component for ASR matching and boundary editing"
  - "StagingQueue component for replacement management"
affects: [12-06, 12-07, 12-08]

# Tech tracking
tech-stack:
  added: []
  patterns: ["Two-column BitGrid layout for Polish workflow (importer left, queue right)"]

key-files:
  created:
    - "host/Ams.Workstation.Server/Components/Pages/Polish/ChapterPolish.razor"
    - "host/Ams.Workstation.Server/Components/Shared/PickupImporter.razor"
    - "host/Ams.Workstation.Server/Components/Shared/StagingQueue.razor"
  modified:
    - "host/Ams.Workstation.Server/Components/Pages/Polish/Index.razor"

key-decisions:
  - "CRX error counts loaded per-chapter from CrxService.GetEntries() filtered by chapter name"
  - "PickupImporter injects PolishService directly to stage matches (co-located with matching logic)"
  - "Pickup waveform preview uses /api/audio/file endpoint pattern for serving arbitrary WAV paths"
  - "Flagged sentences derived from hydrated transcript error status and diff ops presence"

patterns-established:
  - "Polish page pattern: two-column BitGrid(12) with ColumnSpan 7/5 for importer/queue"
  - "Confidence badge coloring: green >= 0.8, yellow >= 0.5, red below"
  - "Status-dependent action buttons: switch on ReplacementStatus for context-sensitive UI"

requirements-completed: [REQ-TAKE, REQ-STAGE]

# Metrics
duration: 8min
completed: 2026-02-23
---

# Phase 12 Plan 05: Polish Page Layout Summary

**Polish area UI with chapter list landing page, per-chapter waveform view, pickup importer with ASR matching and draggable regions, and staging queue with status-based actions**

## Performance

- **Duration:** 8 min
- **Started:** 2026-02-23T10:04:24Z
- **Completed:** 2026-02-23T10:12:42Z
- **Tasks:** 3
- **Files modified:** 4

## Accomplishments
- Replaced Polish area placeholder with functional chapter list showing CRX error counts per chapter
- Built ChapterPolish page with waveform player, pickup importer, and staging queue in two-column layout
- PickupImporter component runs ASR matching, displays confidence-scored results, enables boundary fine-tuning via draggable waveform regions
- StagingQueue component renders per-item cards with status badges and context-sensitive action buttons

## Task Commits

Each task was committed atomically:

1. **Task 1: Polish landing page and ChapterPolish page** - `58e0eee` (feat)
2. **Task 2: PickupImporter component** - `70bacab` (feat)
3. **Task 3: StagingQueue component** - `c8e0a1d` (feat)

## Files Created/Modified
- `host/Ams.Workstation.Server/Components/Pages/Polish/Index.razor` - Polish landing page with chapter list and CRX error counts
- `host/Ams.Workstation.Server/Components/Pages/Polish/ChapterPolish.razor` - Per-chapter polish view with waveform, importer, and queue
- `host/Ams.Workstation.Server/Components/Shared/PickupImporter.razor` - Pickup file import with ASR matching, confidence badges, boundary editing
- `host/Ams.Workstation.Server/Components/Shared/StagingQueue.razor` - Staging queue with status badges and apply/revert/remove actions

## Decisions Made
- CRX error counts loaded per-chapter from CrxService.GetEntries() filtered by chapter name on the landing page
- PickupImporter injects PolishService directly rather than firing callbacks for every staging step, keeping the component self-contained for the import-to-stage flow
- Flagged sentences are derived from hydrated transcript by filtering for error status or non-empty diff ops
- Pickup waveform preview uses a file-path-based audio URL for serving arbitrary WAV files outside the chapter directory

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed IReadOnlyList.Length to .Count**
- **Found during:** Task 1 (ChapterPolish.razor)
- **Issue:** Used `.Length` on `IReadOnlyList<HydratedDiffOp>` which only has `.Count` (Length is array-only)
- **Fix:** Changed `s.Diff?.Ops?.Length > 0` to `(s.Diff?.Ops?.Count ?? 0) > 0`
- **Files modified:** ChapterPolish.razor
- **Verification:** Build succeeded after fix
- **Committed in:** 58e0eee (Task 1 commit)

**2. [Rule 3 - Blocking] Created stub components for build-order dependency**
- **Found during:** Task 1 (ChapterPolish.razor references PickupImporter and StagingQueue)
- **Issue:** ChapterPolish.razor references PickupImporter.razor and StagingQueue.razor which don't exist yet (Tasks 2 and 3)
- **Fix:** Created minimal stub versions with correct parameter signatures to unblock compilation, replaced with full implementations in subsequent tasks
- **Files modified:** PickupImporter.razor, StagingQueue.razor (stubs)
- **Verification:** Task 1 build succeeded with stubs, Tasks 2-3 replaced with full implementations
- **Committed in:** 58e0eee (Task 1 commit, stubs), 70bacab + c8e0a1d (full implementations)

---

**Total deviations:** 2 auto-fixed (1 bug, 1 blocking)
**Impact on plan:** Both fixes necessary for correctness and build order. No scope creep.

## Issues Encountered
- Ams.Workstation.Server.exe process repeatedly locked DLLs during builds (running server instance). Required killing the process between build attempts. Pre-existing environment issue, not caused by plan changes.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Polish area UI complete -- all four Razor components wired to backend services
- Ready for plan 12-06 (Pickup Matching UI enhancements) and 12-07 (Batch Operations)
- All components use BitUI per project conventions (BitCard, BitStack, BitTag, BitButton, BitGrid)

## Self-Check: PASSED

All 5 files verified present. All 3 task commits verified in git history.

---
*Phase: 12-polish-area-foundation*
*Completed: 2026-02-23*
