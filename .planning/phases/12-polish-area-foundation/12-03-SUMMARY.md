---
phase: 12-polish-area-foundation
plan: 03
subsystem: audio, services
tags: [asr, whisper, levenshtein, fuzzy-matching, audio-splice, undo, pickup]

# Dependency graph
requires:
  - phase: 12-01
    provides: "AudioSpliceService for crossfade replacement, PolishModels domain types"
  - phase: 12-02
    provides: "StagingQueueService for queue persistence, UndoService for backup/restore"
provides:
  - "PickupMatchingService: ASR-based pickup-to-sentence matching with silence segmentation"
  - "PolishService: full Polish workflow orchestration (import, stage, apply, revert)"
affects: [12-05, 12-06, 12-07, 12-08]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Static processor pattern for ASR and audio operations (no DI needed)"
    - "Orchestrator service pattern: PolishService coordinates multiple services via DI"

key-files:
  created:
    - "host/Ams.Workstation.Server/Services/PickupMatchingService.cs"
    - "host/Ams.Workstation.Server/Services/PolishService.cs"
  modified:
    - "host/Ams.Workstation.Server/Program.cs"

key-decisions:
  - "ASR model path resolved via AMS_WHISPER_MODEL_PATH env var (AsrEngineConfig.ResolveModelPath)"
  - "PickupMatchingService uses inline text normalization (lowercase, remove punctuation, collapse whitespace) rather than full TextNormalizer to avoid contraction expansion altering ASR comparison fidelity"
  - "PolishService registered as transient (depends on workspace state, stateless orchestrator)"

patterns-established:
  - "Pickup matching: silence-segment -> ASR -> fuzzy match against target sentences"
  - "Apply workflow: backup original BEFORE splice, calculate timing delta after"

requirements-completed: [REQ-TAKE]

# Metrics
duration: 7min
completed: 2026-02-23
---

# Phase 12 Plan 03: Pickup Matching & Polish Orchestration Summary

**ASR-based pickup matching with Whisper.NET and full Polish workflow orchestration (import, stage, apply with undo backup, revert)**

## Performance

- **Duration:** 7 min
- **Started:** 2026-02-23T09:53:58Z
- **Completed:** 2026-02-23T10:00:41Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- PickupMatchingService processes both session files (multi-pickup with silence-based segmentation) and individual pickup files
- Fuzzy matching via Levenshtein similarity with 0.5 confidence threshold ensures only strong matches are accepted
- PolishService orchestrates the complete workflow: import pickups, stage replacements, apply with crossfade splice (backing up originals first), and revert from backup
- Timing delta calculated after each splice for downstream timing shift tracking
- All four Polish services (StagingQueue, Undo, PickupMatching, Polish) registered in DI

## Task Commits

Each task was committed atomically:

1. **Task 1: PickupMatchingService** - `b7f4d1a` (feat)
2. **Task 2: PolishService orchestrator and DI** - `965b6ab` (feat)

## Files Created/Modified
- `host/Ams.Workstation.Server/Services/PickupMatchingService.cs` - ASR-based pickup-to-sentence matching with silence segmentation and fuzzy text matching
- `host/Ams.Workstation.Server/Services/PolishService.cs` - Full Polish workflow orchestrator: import, stage, apply (with undo), revert
- `host/Ams.Workstation.Server/Program.cs` - DI registration for PickupMatchingService and PolishService as transient

## Decisions Made
- Used `AsrEngineConfig.ResolveModelPath(null)` to resolve Whisper model path from environment variable -- consistent with CLI pattern and avoids hardcoded paths
- PickupMatchingService uses simple inline normalization (lowercase + remove punctuation + collapse whitespace) rather than the full TextNormalizer which expands contractions -- ASR output already uses expanded forms, so contraction expansion would reduce match fidelity
- Registered both new services as transient since they are stateless orchestrators (all persistent state lives in the singleton StagingQueue and Undo services)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Added ASR options resolution via AsrEngineConfig**
- **Found during:** Task 1 (PickupMatchingService)
- **Issue:** Plan specified "no DI dependencies" but AsrProcessor.TranscribeBufferAsync requires AsrOptions with a ModelPath. No mechanism was specified for resolving the model path.
- **Fix:** Added BuildAsrOptions() helper that uses AsrEngineConfig.ResolveModelPath(null) to read from AMS_WHISPER_MODEL_PATH environment variable, consistent with CLI approach.
- **Files modified:** PickupMatchingService.cs
- **Verification:** Build succeeds, model path resolves at runtime from env var
- **Committed in:** b7f4d1a (Task 1 commit)

**2. [Rule 2 - Missing Critical] Added original timing boundaries to StageReplacement**
- **Found during:** Task 2 (PolishService)
- **Issue:** Plan's StageReplacement signature used only PickupMatch and pickupFilePath, but StagedReplacement requires OriginalStartSec/OriginalEndSec (the sentence boundaries in chapter audio) which come from HydratedSentence timing, not from PickupMatch.
- **Fix:** Added originalStartSec and originalEndSec parameters to StageReplacement method so callers provide the chapter-side timing boundaries.
- **Files modified:** PolishService.cs
- **Verification:** Build succeeds, StagedReplacement populated with all required fields
- **Committed in:** 965b6ab (Task 2 commit)

---

**Total deviations:** 2 auto-fixed (2 missing critical)
**Impact on plan:** Both auto-fixes necessary for correctness. No scope creep.

## Issues Encountered
- Build initially failed due to locked DLL files from a running Ams.Workstation.Server process -- resolved by killing the process (not a code issue)

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Polish backend services are complete: StagingQueue, Undo, PickupMatching, and PolishService
- Ready for Plan 12-05 (Polish Page Layout) and Plan 12-06 (Pickup Matching UI) which will expose these services through Blazor components
- AMS_WHISPER_MODEL_PATH environment variable must be set for pickup ASR to work at runtime

## Self-Check: PASSED

- All created files verified present on disk
- All commit hashes verified in git log
- Build succeeds with 0 errors

---
*Phase: 12-polish-area-foundation*
*Completed: 2026-02-23*
