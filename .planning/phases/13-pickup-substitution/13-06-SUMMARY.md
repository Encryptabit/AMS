---
phase: 13-pickup-substitution
plan: 06
subsystem: ui, audio
tags: [blazor, roomtone, crossfade, audio-splice, waveform-regions]

# Dependency graph
requires:
  - phase: 13-01
    provides: AudioSpliceService roomtone helpers (GenerateRoomtoneFill, DeleteRegion, InsertAtPoint)
  - phase: 13-03
    provides: PickupSubstitution page with WaveformPlayer and three-column pipeline
  - phase: 13-04
    provides: Cross-chapter processing pipeline and stage/unstage actions
provides:
  - PolishService.ApplyRoomtoneOperationAsync for Insert/Replace/Delete roomtone operations
  - Roomtone Operations UI card with operation buttons and crossfade slider
  - Blue editable waveform region for roomtone target selection
  - Per-operation crossfade duration adjustment (5-200ms)
affects: [13-07, 13-08]

# Tech tracking
tech-stack:
  added: []
  patterns: [roomtone-operation-pattern, per-operation-crossfade, region-color-coding]

key-files:
  modified:
    - host/Ams.Workstation.Server/Services/PolishService.cs
    - host/Ams.Workstation.Server/Components/Pages/Polish/PickupSubstitution.razor

key-decisions:
  - "Roomtone undo uses sentenceId=-1 sentinel since operations are not sentence-specific"
  - "Replacement duration computed per operation type for accurate undo tracking"
  - "Roomtone selection region defaults to 1-2 seconds at waveform start on Set"

patterns-established:
  - "Region color coding: green=staged, blue=roomtone-selection, gray=committed"
  - "Crossfade slider pattern: HTML range input with ms display, converted to seconds for service calls"

requirements-completed: [PS-ROOMTONE]

# Metrics
duration: 8min
completed: 2026-02-24
---

# Phase 13 Plan 06: Roomtone Operations Summary

**Roomtone insert/replace/delete operations via PolishService with per-replacement crossfade slider and blue waveform region selection**

## Performance

- **Duration:** 8 min
- **Started:** 2026-02-24T19:24:45Z
- **Completed:** 2026-02-24T19:33:26Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments
- PolishService.ApplyRoomtoneOperationAsync handles all three roomtone operations using AudioSpliceService helpers
- Roomtone Operations UI card with Insert/Replace/Delete buttons, crossfade slider (5-200ms, default 30ms)
- Blue editable waveform region for selecting roomtone operation target area
- All operations back up original segment via UndoService for potential revert

## Task Commits

Each task was committed atomically:

1. **Task 1: Roomtone operation service method in PolishService** - `db7f24b` (feat)
2. **Task 2: Roomtone UI controls and crossfade slider** - `a2d2d30` (feat)

## Files Created/Modified
- `host/Ams.Workstation.Server/Services/PolishService.cs` - Added ApplyRoomtoneOperationAsync method handling Insert, Replace, Delete with undo backup
- `host/Ams.Workstation.Server/Components/Pages/Polish/PickupSubstitution.razor` - Roomtone Operations card, crossfade slider, blue region, HandleRoomtoneOp handler

## Decisions Made
- Used sentenceId=-1 as sentinel for roomtone operations in UndoService since they are not sentence-specific
- Computed replacementDurationSec per operation type (insert=roomtone fill length, replace=region duration, delete=0) for accurate undo record
- Default roomtone region positioned at 1-2 seconds from start, adjustable via drag

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed replacementDurationSec computation for UndoService**
- **Found during:** Task 1 (ApplyRoomtoneOperationAsync implementation)
- **Issue:** Plan's code passed total buffer length delta as replacementDurationSec, which is incorrect for the UndoRecord schema
- **Fix:** Computed correct replacementDurationSec per operation type: insert=roomtone fill length, replace=region duration, delete=0
- **Files modified:** host/Ams.Workstation.Server/Services/PolishService.cs
- **Verification:** Build passes, UndoRecord schema matches
- **Committed in:** db7f24b (Task 1 commit)

**2. [Rule 3 - Blocking] Removed duplicate stub methods**
- **Found during:** Task 2 (UI implementation)
- **Issue:** Previous plan had already added stub HandleRoomtoneOp and OnCrossfadeInput methods; adding real implementations caused CS0111 duplicate member error
- **Fix:** Removed stub methods at end of file, keeping the full implementations added earlier in the code block
- **Files modified:** host/Ams.Workstation.Server/Components/Pages/Polish/PickupSubstitution.razor
- **Verification:** Build succeeds with 0 errors
- **Committed in:** a2d2d30 (Task 2 commit)

---

**Total deviations:** 2 auto-fixed (1 bug, 1 blocking)
**Impact on plan:** Both fixes necessary for correctness and build success. No scope creep.

## Issues Encountered
None beyond the auto-fixed deviations.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Roomtone operations fully functional, ready for integration testing in plan 13-08
- All three operation types (Insert, Replace, Delete) wire through to AudioSpliceService
- Crossfade is user-adjustable per operation, satisfying the locked decision requirement

## Self-Check: PASSED

All files exist, all commits verified, all key methods present.

---
*Phase: 13-pickup-substitution*
*Completed: 2026-02-24*
