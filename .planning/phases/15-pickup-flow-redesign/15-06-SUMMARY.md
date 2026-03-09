---
phase: 15-pickup-flow-redesign
plan: 06
subsystem: ui, polish
tags: [blazor, pickup-substitution, unmatched-bucket, reassignment, projection-mapping, trim-handles, migration]

requires:
  - phase: 15-pickup-flow-redesign
    provides: "PickupAssetService.ImportAsync, StagingQueueService.GetCurrentTime, PolishService.GenerateContextPlaybackPreview, PickupAsset model, EditListService"
provides:
  - "Unified import UI via PickupAssetService with matched/unmatched split"
  - "Unmatched bucket with manual CRX target assignment"
  - "Reassignment of any matched pickup to a different CRX target"
  - "Projection-mapped waveform region positions via TimelineProjection"
  - "Pickup detail panel with trim handles (orange regions)"
  - "Context playback audition on staged items"
  - "Migration detection for old staging queue format"
affects: [15-07]

tech-stack:
  added: []
  patterns:
    - "Projection-mapped regions: all waveform region positions mapped through StagingQueueService.GetCurrentTime"
    - "Baseline coordinate conversion: newBaseline = oldBaseline + (newCurrent - oldCurrent) for region handle adjustments"
    - "Dual-waveform pattern: chapter waveform + pickup detail panel waveform for separate trim editing"

key-files:
  created: []
  modified:
    - host/Ams.Workstation.Server/Components/Pages/Polish/PickupSubstitution.razor
    - host/Ams.Workstation.Server/Components/Shared/PickupBox.razor
    - host/Ams.Workstation.Server/Models/PolishModels.cs
    - host/Ams.Workstation.Server/Services/StagingQueueService.cs

key-decisions:
  - "Unmatched bucket placed within Matches column (below matched items) rather than a 4th column for layout efficiency"
  - "Reassignment uses target selector dropdown on PickupBox rather than a separate modal dialog"
  - "Region handle adjustments convert current-time back to baseline: newBaseline = oldBaseline + (newCurrent - oldCurrent)"
  - "Migration detection is read-only check: old queue items + no edit list = show warning"
  - "Context playback via GenerateContextPlaybackPreview rather than standalone audition for staged items"

patterns-established:
  - "Pickup detail panel: click any PickupBox to open full waveform with trim region handles"
  - "Migration detection: check old staging queue against new edit list for clean-slate prompt"

requirements-completed: [PFR-DUAL, PFR-REASSIGN]

duration: 6min
completed: 2026-03-09
---

# Phase 15 Plan 06: PickupSubstitution UI Refactor Summary

**Unified import with unmatched bucket, manual CRX reassignment, projection-mapped waveform regions, pickup trim panel, and old-format migration detection**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-09T10:16:25Z
- **Completed:** 2026-03-09T10:23:24Z
- **Tasks:** 3
- **Files modified:** 4

## Accomplishments
- PickupAssetService.ImportAsync integrated for unified pickup import, returning matched and unmatched asset lists
- Unmatched bucket UI with target selector dropdown for manual CRX target assignment
- Any matched pickup can be reassigned to a different CRX target via Reassign button + dropdown
- Waveform regions display at projection-mapped positions via StagingQueueService.GetCurrentTime
- Chapter-side boundary handle adjustments convert current-time positions back to baseline coordinates
- Pickup detail panel opens on PickupBox click with orange trim region handles on a separate WaveformPlayer
- Context playback audition via PolishService.GenerateContextPlaybackPreview for staged items
- Migration detection warns users with old staging queue data that lacks edit list entries

## Task Commits

Each task was committed atomically:

1. **Task 1: Unified import + unmatched bucket + manual reassignment** - `9bc6887` (feat)
2. **Task 2: Dual-side boundary editing + projection-mapped regions** - `c6b9ae0` (feat)
3. **Task 3: Migration detection + old staging queue cleanup** - `9a41725` (feat)

## Files Created/Modified
- `host/Ams.Workstation.Server/Components/Pages/Polish/PickupSubstitution.razor` - Refactored with unified import, unmatched bucket, pickup detail panel, projection-mapped regions, migration detection
- `host/Ams.Workstation.Server/Components/Shared/PickupBox.razor` - Enhanced with Asset, AvailableTargets, OnAssign, OnReassign, OnSelect, OnAuditionContext params; Unmatched state UI
- `host/Ams.Workstation.Server/Models/PolishModels.cs` - Added PickupBoxState.Unmatched enum value
- `host/Ams.Workstation.Server/Services/StagingQueueService.cs` - Added TryUpdatePickupBoundaries for pickup-side trim handle updates

## Decisions Made
- **Unmatched bucket placement:** Within the Matches column below matched items, rather than a separate 4th column. This keeps the three-column pipeline layout intact while still surfacing unmatched pickups prominently.
- **Reassignment mechanism:** Target selector dropdown directly on PickupBox rather than a modal dialog. Simpler UX for the common case where there are few available targets.
- **Baseline coordinate conversion:** When user drags a region handle in current-time space, the delta is applied to the baseline coordinates: `newBaseline = oldBaseline + (newCurrent - oldCurrent)`. This preserves the baseline-coordinate invariant while allowing user adjustment.
- **Context playback:** Staged items use `GenerateContextPlaybackPreview` (±2.0s context) rather than standalone pickup audition, giving users the splice-in-context experience.
- **Migration detection:** Read-only check — if old staging queue has items but new edit list is empty, show a warning banner. No auto-migration of coordinate systems.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- PickupSubstitution.razor fully refactored with all Phase 15 capabilities surfaced
- Ready for Plan 15-07 (Integration & Cleanup) for final verification and polish

## Self-Check: PASSED

- FOUND: host/Ams.Workstation.Server/Components/Pages/Polish/PickupSubstitution.razor
- FOUND: host/Ams.Workstation.Server/Components/Shared/PickupBox.razor
- FOUND: host/Ams.Workstation.Server/Models/PolishModels.cs
- FOUND: host/Ams.Workstation.Server/Services/StagingQueueService.cs
- FOUND: 9bc6887 (Task 1 commit)
- FOUND: c6b9ae0 (Task 2 commit)
- FOUND: 9a41725 (Task 3 commit)

---
*Phase: 15-pickup-flow-redesign*
*Completed: 2026-03-09*
