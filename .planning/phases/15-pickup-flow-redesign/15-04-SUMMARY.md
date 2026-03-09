---
phase: 15-pickup-flow-redesign
plan: 04
subsystem: audio, polish
tags: [staging-queue, undo-service, baseline-coordinates, edit-list, timeline-projection, rebuild-revert]

requires:
  - phase: 15-pickup-flow-redesign
    provides: "ChapterEdit domain model, EditListService, TimelineProjection"
provides:
  - "StagingQueueService with immutable baseline coordinates and EditListService integration"
  - "UndoService with replacement segment storage for rebuild-based revert"
  - "GetCurrentTime helper for baseline→current timeline queries"
affects: [15-05, 15-06, 15-07]

tech-stack:
  added: []
  patterns:
    - "Baseline-only coordinates: StagedReplacement boundaries never mutated after staging"
    - "Edit list integration: ChapterEdit created on apply, removed on revert"
    - "Rebuild-based undo: replacement audio segments persisted alongside originals"

key-files:
  created: []
  modified:
    - host/Ams.Workstation.Server/Services/StagingQueueService.cs
    - host/Ams.Workstation.Server/Services/UndoService.cs
    - host/Ams.Workstation.Server/Models/PolishModels.cs
    - host/Ams.Workstation.Server/Services/PolishService.cs

key-decisions:
  - "ShiftDownstream fully removed — baseline coordinates are immutable, timeline mapping via TimelineProjection"
  - "ChapterEdit created inside UpdateStatus for tight coupling between status transition and edit tracking"
  - "UndoRecord extended with optional ReplacementSegmentPath for backward-compatible manifest format"

patterns-established:
  - "Immutable staging coordinates: OriginalStartSec/OriginalEndSec set once at staging, never modified"
  - "Edit lifecycle in UpdateStatus: Applied→Add ChapterEdit, Reverted→Remove ChapterEdit"

requirements-completed: [PFR-EDITLIST]

duration: 4min
completed: 2026-03-09
---

# Phase 15 Plan 04: StagingQueue & UndoService Refactor Summary

**Immutable baseline coordinates in StagingQueueService with EditListService integration, and UndoService enhanced with replacement segment storage for rebuild-based revert**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-09T10:00:14Z
- **Completed:** 2026-03-09T10:04:56Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- Removed ShiftDownstream method and all calls from StagingQueueService and PolishService — baseline coordinates are now immutable
- Integrated EditListService into StagingQueueService: ChapterEdit records created on apply, removed on revert, via UpdateStatus
- Added GetCurrentTime helper that delegates to TimelineProjection.BaselineToCurrentTime for timeline queries
- Added SaveReplacementSegmentAsync and LoadReplacementSegment to UndoService for rebuild-based revert
- Extended UndoRecord with optional ReplacementSegmentPath for backward-compatible manifest format

## Task Commits

Each task was committed atomically:

1. **Task 1: Refactor StagingQueueService** - `acc380b` (feat)
2. **Task 2: Enhance UndoService for rebuild** - `291c893` (feat)

## Files Created/Modified
- `host/Ams.Workstation.Server/Services/StagingQueueService.cs` - Removed ShiftDownstream, injected EditListService, added GetCurrentTime helper, integrated ChapterEdit lifecycle in UpdateStatus
- `host/Ams.Workstation.Server/Services/UndoService.cs` - Added SaveReplacementSegmentAsync, LoadReplacementSegment, UpdateRecordReplacementPath private helper
- `host/Ams.Workstation.Server/Models/PolishModels.cs` - Extended UndoRecord with optional ReplacementSegmentPath field
- `host/Ams.Workstation.Server/Services/PolishService.cs` - Removed ShiftDownstream calls in apply and revert paths

## Decisions Made
- **ChapterEdit lifecycle in UpdateStatus:** Tight coupling between status transitions and edit list ensures no edit can be applied without being tracked, and no revert can skip removal. This is simpler and more reliable than requiring callers to manage edit list separately.
- **Optional ReplacementSegmentPath:** Using optional parameter (default null) keeps the UndoRecord backward-compatible — existing manifests without replacement paths deserialize without error.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Removed ShiftDownstream calls from PolishService**
- **Found during:** Task 1 (StagingQueueService refactor)
- **Issue:** PolishService.cs called ShiftDownstream in both ApplyReplacementAsync and RevertReplacementAsync — removing the method from StagingQueueService caused build failure
- **Fix:** Removed both ShiftDownstream calls from PolishService since edit list tracking in UpdateStatus makes them redundant
- **Files modified:** host/Ams.Workstation.Server/Services/PolishService.cs
- **Verification:** Build succeeds with 0 errors
- **Committed in:** acc380b (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Necessary to compile. PolishService was the only caller of ShiftDownstream — removing those calls is the direct consequence of removing the method.

## Issues Encountered

None

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- StagingQueueService now uses immutable baseline coordinates and integrates with EditListService
- UndoService ready to persist both original and replacement audio segments for rebuild
- Ready for Plan 15-05 (Dual-Side Handle Editing) which will use GetCurrentTime for real-time timeline queries

## Self-Check: PASSED

All modified files verified on disk. All task commits verified in git history.

---
*Phase: 15-pickup-flow-redesign*
*Completed: 2026-03-09*
