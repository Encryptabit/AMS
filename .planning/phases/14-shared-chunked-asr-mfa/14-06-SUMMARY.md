---
phase: 14-shared-chunked-asr-mfa
plan: 06
subsystem: alignment
tags: [mfa, beam-search, profiles, adaptive-retry, chunked-alignment]

requires:
  - phase: 14-04
    provides: MfaChunkCorpusBuilder and chunked corpus path
  - phase: 14-05
    provides: TextGridAggregationService for chunk-to-chapter merging
provides:
  - MFA beam profile enum (Fast/Balanced/Strict) with configurable resolution
  - Adaptive strict retry on low-quality chunk subsets
  - CLI flags for --mfa-profile and beam overrides
  - Pickup MFA refinement aligned to profile conventions
affects: [14-07, pipeline-tuning, workstation-mfa]

tech-stack:
  added: []
  patterns: [beam-profile-resolution, chunk-quality-detection, selective-retry]

key-files:
  created: []
  modified:
    - host/Ams.Core/Application/Mfa/Models/MfaBeamProfile.cs
    - host/Ams.Core/Application/Mfa/MfaWorkflow.cs
    - host/Ams.Core/Application/Commands/RunMfaCommand.cs
    - host/Ams.Cli/Commands/PipelineCommand.cs
    - host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs

key-decisions:
  - "Coverage heuristic: 0.15 ratio of actual/expected words (3.3 words/sec estimate) triggers low-coverage retry"
  - "Retry re-aligns full corpus with strict beam, collects only failed chunk TextGrids to avoid partial corpus issues"
  - "Retry skipped when initial beam >= strict to avoid redundant work"
  - "Pickup MFA refinement uses MfaBeamSettings.Resolve(Strict) instead of hardcoded beam=80/retry=200"

patterns-established:
  - "Beam profile resolution: MfaBeamSettings.Resolve(profile, explicitBeam?, explicitRetryBeam?) with override precedence"
  - "Chunk quality detection: CollectChunkTextGrids evaluates missing/parse-failure/low-coverage per chunk"
  - "Selective retry: only problematic chunks are re-collected after strict beam re-alignment"

requirements-completed: [MFA-PROFILES]

duration: 2min
completed: 2026-03-05
---

# Phase 14 Plan 06: MFA Beam Profiles and Adaptive Retry Summary

**Configurable MFA beam profiles (fast/balanced/strict) with per-chunk quality detection and selective strict retry for failed alignments**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-05T06:42:47Z
- **Completed:** 2026-03-05T06:45:22Z
- **Tasks:** 2
- **Files modified:** 5

## Accomplishments
- MFA beam/retry-beam externally configurable via profiles (Fast=20/80, Balanced=40/120, Strict=80/200) with explicit override support
- Chunk-level quality detection after initial alignment: missing output, parse failure, low coverage heuristic
- Adaptive strict retry re-aligns only problematic chunks, preserving successful alignments
- Pickup MFA refinement service aligned to profile conventions (no more hardcoded beam constants)

## Task Commits

Each task was committed atomically:

1. **Task 1: Add MFA profile/config surface** - `c00c559` (feat)
2. **Task 2: Adaptive strict retry on low-quality subsets** - `0cf950d` (feat)

## Files Created/Modified
- `host/Ams.Core/Application/Mfa/Models/MfaBeamProfile.cs` - MfaBeamProfile enum + MfaBeamSettings resolver with StrictRetry static property
- `host/Ams.Core/Application/Mfa/MfaWorkflow.cs` - Chunk quality detection (CollectChunkTextGrids), adaptive strict retry (RetryFailedChunksWithStrictBeamAsync), FindUtteranceTextGrid helper
- `host/Ams.Core/Application/Commands/RunMfaCommand.cs` - RunMfaOptions with BeamProfile/Beam/RetryBeam properties, resolution via MfaBeamSettings
- `host/Ams.Cli/Commands/PipelineCommand.cs` - CLI flags for --mfa-profile and beam/retry-beam overrides
- `host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs` - Uses MfaBeamSettings.Resolve(Strict) instead of hardcoded beam=80/retry=200

## Decisions Made
- Coverage heuristic threshold of 0.15: ~3.3 words/sec average speech rate means a 30-second chunk expects ~100 words; below 15 actual words triggers retry
- Retry re-runs full corpus alignment with strict beam rather than creating subset corpus directory -- MFA manages its own utterance-level recovery, and subset corpus introduces file management complexity
- When initial beam is already at strict level, retry is skipped to avoid redundant work
- PickupMfaRefinementService now uses profile-based resolution for consistency; beam values are identical (80/200) but now derived from the canonical Strict profile

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- MFA beam strategy is fully configurable from CLI and code
- Adaptive retry provides automatic recovery path for difficult chunks
- Ready for plan 14-07 (final integration/verification)

---
*Phase: 14-shared-chunked-asr-mfa*
*Completed: 2026-03-05*
