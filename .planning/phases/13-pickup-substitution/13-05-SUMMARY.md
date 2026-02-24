---
phase: 13-pickup-substitution
plan: 05
subsystem: ui
tags: [blazor, wavesurfer, waveform-regions, audio-splice, pickup-substitution]

# Dependency graph
requires:
  - phase: 13-03
    provides: "PickupSubstitution page with three-column pipeline and PickupBox component"
  - phase: 13-04
    provides: "Cross-chapter import pipeline, stage/unstage/drag-and-drop actions"
  - phase: 12-04
    provides: "WaveformPlayer AddEditableRegion and region callback infrastructure"
  - phase: 12-02
    provides: "StagingQueueService and UndoService for commit/revert flow"
provides:
  - "Waveform region editing for staged pickups with draggable boundaries"
  - "Commit flow writing corrected.wav with format-preserving encode"
  - "Revert flow restoring audio via UndoService backup"
  - "Completion tracking with auto-advance to next incomplete chapter"
  - "ClearRegions, RemoveRegion, AddRegion public methods on WaveformPlayer"
  - "UpdateBoundaries method on StagingQueueService"
affects: [13-06, 13-07, 13-08]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Region sync pattern: ClearRegions + AddEditableRegion/AddRegion on each state change"
    - "Completion-driven auto-advance via FindNextIncompleteChapter wrap-around search"
    - "Cache-bust corrected audio URL with _correctedVersion increment"

key-files:
  created: []
  modified:
    - "host/Ams.Workstation.Server/Components/Pages/Polish/PickupSubstitution.razor"
    - "host/Ams.Workstation.Server/Components/Shared/WaveformPlayer.razor"
    - "host/Ams.Workstation.Server/Components/Shared/PickupBox.razor"
    - "host/Ams.Workstation.Server/Services/StagingQueueService.cs"

key-decisions:
  - "Region sync on every state change: clear-all + re-add pattern ensures consistency vs incremental updates"
  - "Committed regions use non-editable AddRegion to prevent accidental boundary modification"
  - "Commit button placed as primary (Fill+Success) in PickupBox Staged state for discoverability"
  - "Roomtone operations added as stubs to unblock plan 13-07 without blocking build"

patterns-established:
  - "ClearRegions + re-add: ensures waveform region state stays in sync with staging queue"
  - "CheckCompletionAndAdvance: wrap-around search for next incomplete chapter with 500ms visual delay"

requirements-completed: [PS-REGIONS, PS-COMMIT, PS-COMPLETE]

# Metrics
duration: 7min
completed: 2026-02-24
---

# Phase 13 Plan 05: Region Editing, Commit/Revert, Completion Tracking Summary

**Waveform region editing for staged pickups with draggable green boundaries, commit/revert flow writing format-preserving corrected.wav, completion badges, and auto-advance to next incomplete chapter**

## Performance

- **Duration:** 7 min
- **Started:** 2026-02-24T19:24:36Z
- **Completed:** 2026-02-24T19:31:37Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- Staged pickups display as green draggable regions on the chapter waveform; committed items show as gray non-editable regions
- Boundary drags persist to StagingQueueService.UpdateBoundaries and update the splice target
- Commit (individual or batch) writes corrected.wav via PolishService.ApplyReplacementAsync and reloads the waveform via cache-busted URL
- Revert restores original audio from UndoService backup and moves box back to Matches column
- Completion badge (BitTag Success "Complete") appears when all CRX entries for a chapter are committed
- Auto-advance navigates to next incomplete chapter after completion, with wrap-around search
- Audition plays pickup preview via GeneratePreview + PlaySegment

## Task Commits

Each task was committed atomically:

1. **Task 1: Staged pickup regions on chapter waveform with boundary editing** - `b582bf7` (feat)
2. **Task 2: Commit/revert flow, waveform reload, completion tracking, auto-advance** - `a6ed80c` (feat)

## Files Created/Modified
- `host/Ams.Workstation.Server/Components/Pages/Polish/PickupSubstitution.razor` - Region editing, commit/revert/commit-all flow, completion tracking, auto-advance, roomtone stubs
- `host/Ams.Workstation.Server/Components/Shared/WaveformPlayer.razor` - ClearRegions, RemoveRegion, AddRegion public methods
- `host/Ams.Workstation.Server/Components/Shared/PickupBox.razor` - Commit button in Staged state, HandleCommit handler
- `host/Ams.Workstation.Server/Services/StagingQueueService.cs` - UpdateBoundaries method for drag-driven boundary edits

## Decisions Made
- Used clear-all + re-add pattern for region sync rather than incremental add/remove, trading minor overhead for guaranteed consistency
- Committed regions rendered as non-editable (AddRegion vs AddEditableRegion) to prevent accidental modification
- Commit button styled as primary Fill+Success in PickupBox for discoverability alongside Unstage+Audition
- Added roomtone operation stubs (HandleRoomtoneOp, OnCrossfadeInput) to support linter-added roomtone UI section without breaking build

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added roomtone operation stubs for linter-generated UI**
- **Found during:** Task 2 (build verification)
- **Issue:** Linter auto-added roomtone operations UI section (Row 4) with references to HandleRoomtoneOp and OnCrossfadeInput methods that did not yet exist
- **Fix:** Added stub methods that display "not yet implemented" status messages; added roomtone state fields (_roomtoneRegionStart, _roomtoneRegionEnd, _roomtoneRegionValid, _crossfadeDurationMs)
- **Files modified:** host/Ams.Workstation.Server/Components/Pages/Polish/PickupSubstitution.razor
- **Verification:** Build passes with 0 errors
- **Committed in:** a6ed80c (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Stub methods added to unblock build; no scope creep. Roomtone implementation deferred to plan 13-07 as designed.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Region editing and commit/revert flow complete, ready for plan 13-06 (Staging & Commit UI enhancements)
- Roomtone stubs in place, ready for plan 13-07 implementation
- All must-have truths satisfied: green draggable regions, boundary adjustment, audition, commit writes corrected.wav, revert via undo, completion badge, auto-advance

## Self-Check: PASSED

All files verified present. All commits verified in git log. Build passes with 0 errors.

---
*Phase: 13-pickup-substitution*
*Completed: 2026-02-24*
