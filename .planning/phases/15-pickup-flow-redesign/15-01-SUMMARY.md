---
phase: 15-pickup-flow-redesign
plan: 01
subsystem: audio, polish
tags: [timeline-projection, immutable-records, edit-list, pickup-asset, sealed-records]

requires:
  - phase: 12-polish-area
    provides: "AudioSpliceService, StagingQueueService patterns"
  - phase: 13-pickup-substitution
    provides: "PolishModels.cs, Polish area service infrastructure"
provides:
  - "ChapterEdit and EditOperation domain models in Ams.Core.Audio"
  - "TimelineProjection static service for baseline→current time mapping"
  - "EditListService for append-only edit list with JSON persistence"
  - "PickupAsset and PickupAssetCache models for unified pickup import"
affects: [15-02, 15-03, 15-04, 15-05, 15-06, 15-07]

tech-stack:
  added: []
  patterns:
    - "Immutable edit list with projection (replaces mutable ShiftDownstream)"
    - "Core domain models in Ams.Core.Audio, workflow models in Workstation"

key-files:
  created:
    - host/Ams.Core/Audio/ChapterEditModels.cs
    - host/Ams.Core/Audio/TimelineProjection.cs
    - host/Ams.Workstation.Server/Services/EditListService.cs
  modified:
    - host/Ams.Workstation.Server/Models/PolishModels.cs
    - host/Ams.Workstation.Server/Program.cs

key-decisions:
  - "ChapterEdit and EditOperation placed in Ams.Core.Audio (domain models, no UI dependency)"
  - "PickupAsset/PickupAssetCache/PickupSourceType kept in Workstation PolishModels (import-workflow specific)"
  - "EditListService follows StagingQueueService singleton + lazy-load + JSON persistence pattern"

patterns-established:
  - "Baseline-only time references: all ChapterEdit records reference original timeline positions"
  - "Projection-based timeline: TimelineProjection walks edit list to compute current positions"

requirements-completed: [PFR-MODELS, PFR-EDITLIST, PFR-TIMELINE]

duration: 3min
completed: 2026-03-09
---

# Phase 15 Plan 01: Domain Models & Timeline Projection Summary

**Immutable ChapterEdit records with static TimelineProjection service and EditListService for append-only edit list persistence**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-09T09:29:01Z
- **Completed:** 2026-03-09T09:32:08Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- ChapterEdit and EditOperation sealed records defined in Ams.Core.Audio — immutable domain models for the edit list architecture
- TimelineProjection static service with BaselineToCurrentTime and ProjectedDuration methods — maps baseline positions to current post-edit positions by walking the edit list
- EditListService singleton with Add/Remove/GetEdits/GetAllEdits/Clear/HasEdits API, JSON persistence to `.polish/edit-list.json`, thread-safe via lock
- PickupAsset, PickupAssetCache, and PickupSourceType models added to PolishModels.cs — unified import model for session segments and individual files

## Task Commits

Each task was committed atomically:

1. **Task 1: Domain models + TimelineProjection** - `aae4079` (feat)
2. **Task 2: EditListService with JSON persistence** - `dac2b3b` (feat)

## Files Created/Modified
- `host/Ams.Core/Audio/ChapterEditModels.cs` - ChapterEdit sealed record and EditOperation enum (domain layer)
- `host/Ams.Core/Audio/TimelineProjection.cs` - Pure static baseline→current time mapper
- `host/Ams.Workstation.Server/Services/EditListService.cs` - Append-only edit list with JSON persistence
- `host/Ams.Workstation.Server/Models/PolishModels.cs` - Added PickupAsset, PickupAssetCache, PickupSourceType
- `host/Ams.Workstation.Server/Program.cs` - EditListService DI registration

## Decisions Made
- **ChapterEdit in Ams.Core.Audio:** Domain model with no UI dependencies, so placing in Core allows TimelineProjection to reference it without circular dependency. PickupAsset stays in Workstation since it's import-workflow specific.
- **EditListService follows StagingQueueService pattern:** Singleton + lazy-load + JSON persistence to `.polish/` directory. Consistent with existing workstation service patterns. Coexists with StagingQueueService during transition period.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Foundation models and services ready for Phase 15-02 (PickupAssetService and import workflow)
- TimelineProjection ready for integration into PolishService (Phase 15-03+)
- EditListService ready to receive edits from refactored apply/revert flow
- All existing functionality preserved — old models and StagingQueueService untouched

## Self-Check: PASSED

All created files verified on disk. All task commits verified in git history.

---
*Phase: 15-pickup-flow-redesign*
*Completed: 2026-03-09*
