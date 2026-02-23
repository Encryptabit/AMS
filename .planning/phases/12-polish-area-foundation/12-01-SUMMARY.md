---
phase: 12-polish-area-foundation
plan: 01
subsystem: audio, ui
tags: [ffmpeg, acrossfade, splice, crossfade, domain-models, blazor]

# Dependency graph
requires:
  - phase: 09-blazor-workstation
    provides: "Blazor Server project structure and Ams.Core integration"
provides:
  - "AudioSpliceService for crossfaded audio segment replacement"
  - "Polish domain models (StagedReplacement, PickupMatch, UndoRecord, BatchOperation)"
affects: [12-02, 12-03, 12-04, 12-05, 12-06, 12-07, 12-08, 13-pickup-substitution]

# Tech tracking
tech-stack:
  added: []
  patterns: ["Static service with FFmpeg acrossfade for audio splicing", "Sealed records for immutable Polish workflow models"]

key-files:
  created:
    - host/Ams.Core/Audio/AudioSpliceService.cs
  modified:
    - host/Ams.Workstation.Server/Models/PolishModels.cs

key-decisions:
  - "AudioSpliceService is static/stateless following AudioProcessor pattern"
  - "Crossfade via FFmpeg acrossfade filter rather than manual sample math"
  - "Crossfade clamped to 30% of shorter segment to prevent boundary overflow"

patterns-established:
  - "Audio splice pattern: Trim before/after, resample if needed, acrossfade joins"
  - "Polish models as sealed records with enum status lifecycle"

requirements-completed: [REQ-SPLICE, REQ-STAGE]

# Metrics
duration: 5min
completed: 2026-02-23
---

# Phase 12 Plan 01: Audio Splice Service & Polish Domain Models Summary

**AudioSpliceService with FFmpeg acrossfade splicing plus complete Polish workflow domain models (StagedReplacement, PickupMatch, UndoRecord, BatchOperation)**

## Performance

- **Duration:** 5 min
- **Started:** 2026-02-23T09:45:25Z
- **Completed:** 2026-02-23T09:50:25Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments
- AudioSpliceService.ReplaceSegment splices replacement audio into original with crossfade at both join points via FFmpeg acrossfade
- Crossfade duration safely clamped to 30% of shorter segment; auto-resamples on sample rate mismatch
- All Polish domain models defined: StagedReplacement, PickupMatch, PickupSegment, UndoRecord, BatchOperation, BatchTarget with ReplacementStatus and BatchOperationType enums

## Task Commits

Each task was committed atomically:

1. **Task 1: AudioSpliceService in Ams.Core** - `ff4179c` (feat)
2. **Task 2: Polish domain models** - `f35436c` (feat)

## Files Created/Modified
- `host/Ams.Core/Audio/AudioSpliceService.cs` - Static service providing ReplaceSegment with crossfaded audio splicing via FfFilterGraphRunner
- `host/Ams.Workstation.Server/Models/PolishModels.cs` - Sealed records and enums for the Polish workflow domain

## Decisions Made
- AudioSpliceService follows the static/stateless pattern established by AudioProcessor (no DI registration needed)
- Used FfFilterGraphRunner.Apply with multi-input acrossfade filter spec rather than manual sample-level crossfade math
- Crossfade clamp uses 30% of shorter segment (research Pitfall 2) to prevent boundary overflow
- Falls back to AudioBuffer.Concat for negligible crossfade (<=1ms) or empty buffers

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- AudioSpliceService is ready for use by Polish area services (pickup replacement, batch editing)
- All domain models available for Polish API endpoints, state management, and UI binding
- Solution builds cleanly with 0 errors

## Self-Check: PASSED

- [x] `host/Ams.Core/Audio/AudioSpliceService.cs` - FOUND
- [x] `host/Ams.Workstation.Server/Models/PolishModels.cs` - FOUND
- [x] Commit `ff4179c` (Task 1) - FOUND
- [x] Commit `f35436c` (Task 2) - FOUND

---
*Phase: 12-polish-area-foundation*
*Completed: 2026-02-23*
