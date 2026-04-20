---
phase: 15-pickup-flow-redesign
plan: 05
subsystem: audio, polish
tags: [rebuild, revert, timeline-projection, breath-aware, context-playback, unified-pipeline]

requires:
  - phase: 15-pickup-flow-redesign
    provides: "ChapterEdit/EditOperation domain models, TimelineProjection, EditListService, SpliceBoundaryService.RefineBoundariesBreathAware, UndoService.SaveReplacementSegmentAsync/LoadReplacementSegment"
provides:
  - "RebuildChapterAsync — deterministic chapter audio from treated baseline + edit list"
  - "Unified ChapterEdit pipeline for both pickups and roomtone operations"
  - "MapBaselineToCurrentTime — TimelineProjection integration in PolishService"
  - "GenerateContextPlaybackPreview — audition with surrounding chapter audio"
  - "Content-aware handle sizing (crossfadeDuration + HandleGuardSec)"
affects: [15-06, 15-07]

tech-stack:
  added: []
  patterns:
    - "Rebuild-from-baseline: all mutations rebuild from treated audio + back-to-front edit application"
    - "Unified edit pipeline: pickups and roomtone both create ChapterEdit records"
    - "Content-aware handles: handle size = crossfadeDuration + guard, not fixed constant"

key-files:
  created: []
  modified:
    - host/Ams.Workstation.Server/Services/PolishService.cs

key-decisions:
  - "RebuildChapterAsync sorts edits descending by BaselineStartSec (back-to-front) so each edit only affects content after itself"
  - "Apply saves replacement audio via UndoService before updating status, ensuring rebuild can always re-apply"
  - "Revert removes edit from list first, then rebuilds — order matters for correctness"
  - "Roomtone operations create ChapterEdit records directly in PolishService rather than going through StagingQueueService"
  - "HandleGuardSec (30ms) plus crossfade duration replaces fixed 80ms PickupSlicePaddingSec"

patterns-established:
  - "Rebuild-based revert: any edit can be reverted regardless of application order"
  - "Unified edit pipeline: all audio mutations (pickup, roomtone insert/replace/delete) produce ChapterEdit records"

requirements-completed: [PFR-REVERT, PFR-TIMELINE, PFR-CONTEXT]

duration: 5min
completed: 2026-03-09
---

# Phase 15 Plan 05: PolishService Refactor Summary

**Rebuild-from-baseline chapter audio via RebuildChapterAsync with unified ChapterEdit pipeline for pickups and roomtone, breath-aware boundaries, and context playback preview**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-09T10:08:30Z
- **Completed:** 2026-03-09T10:13:43Z
- **Tasks:** 2
- **Files modified:** 1

## Accomplishments
- RebuildChapterAsync applies edits back-to-front against original treated audio — deterministic and correct by construction regardless of revert order
- ApplyReplacementAsync and RevertReplacementAsync both rebuild from baseline, eliminating fragile surgical splice-back revert
- ApplyRoomtoneOperationAsync creates ChapterEdit records through the unified edit pipeline (insert, replace, delete) with replacement audio saved for rebuild
- RebaseTranscriptTimeToCurrentTimeline replaced with MapBaselineToCurrentTime delegating to TimelineProjection.BaselineToCurrentTime via EditListService
- Breath-aware boundary placement via RefineBoundariesBreathAware for initial splice point selection
- Content-aware handle sizing: crossfadeDuration + HandleGuardSec (30ms) replaces fixed 80ms PickupSlicePaddingSec
- GenerateContextPlaybackPreview generates audition clips with ±2.0s of surrounding chapter audio

## Task Commits

Each task was committed atomically:

1. **Task 1: RebuildChapterAsync + unified apply/revert pipeline** - `bc130d9` (feat)
2. **Task 2: Breath-aware boundaries + content-aware handle sizing** - `1e16e0d` (feat)

## Files Created/Modified
- `host/Ams.Workstation.Server/Services/PolishService.cs` - Major refactor: RebuildChapterAsync, unified ChapterEdit pipeline, TimelineProjection integration, breath-aware boundaries, context playback preview (306 insertions, 119 deletions)

## Decisions Made
- **Back-to-front edit application:** Sorting edits descending by BaselineStartSec ensures each edit only modifies audio content downstream of itself, preserving upstream byte positions. This is the standard technique for non-destructive edit lists.
- **Roomtone edits directly in PolishService:** Unlike pickup replacements (which go through StagingQueueService.UpdateStatus), roomtone operations add ChapterEdit records directly because they don't have a staging/applied lifecycle.
- **HandleGuardSec (30ms) + crossfade:** For a default 70ms crossfade, the handle extends 100ms beyond speech edges. This ensures the entire crossfade transition occurs in non-speech audio, preventing audible artifacts.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- PolishService fully refactored with rebuild-based revert and unified edit pipeline
- Ready for Plan 15-06 (Context Playback & Audition) which can build on GenerateContextPlaybackPreview
- Ready for Plan 15-07 (Integration & Cleanup) for final integration testing

## Self-Check: PASSED

- FOUND: host/Ams.Workstation.Server/Services/PolishService.cs
- FOUND: bc130d9 (Task 1 commit)
- FOUND: 1e16e0d (Task 2 commit)

---
*Phase: 15-pickup-flow-redesign*
*Completed: 2026-03-09*
