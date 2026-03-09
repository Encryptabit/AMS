---
phase: 15-pickup-flow-redesign
plan: 03
subsystem: audio, polish
tags: [pickup-import, text-similarity, levenshtein, mfa-segmentation, caching, greedy-matching]

requires:
  - phase: 15-pickup-flow-redesign
    provides: "PickupAsset/PickupAssetCache models from 15-01, PickupMatchingService from Phase 12"
  - phase: 12-polish-area
    provides: "PickupMatchingService, PolishModels, LevenshteinMetrics"
provides:
  - "PickupAssetService for unified pickup import (session file, individual file, folder)"
  - "MatchByTextSimilarity for out-of-order session file matching"
  - "MFA-aware utterance segmentation with configurable gap threshold"
  - "Disk-cached pickup asset processing with CRX fingerprint invalidation"
affects: [15-04, 15-05, 15-06, 15-07]

tech-stack:
  added: []
  patterns:
    - "Greedy best-first text similarity matching (O(n²·m), negligible for 5-30 items)"
    - "Dual-threshold segmentation: 0.4s for MFA-refined, 0.8s for raw ASR"
    - "Unified import with auto-detection of source type"

key-files:
  created:
    - host/Ams.Workstation.Server/Services/PickupAssetService.cs
  modified:
    - host/Ams.Workstation.Server/Services/PickupMatchingService.cs
    - host/Ams.Workstation.Server/Program.cs

key-decisions:
  - "PickupAssetService registered as transient (stateless orchestrator, consistent with PolishService pattern)"
  - "Text similarity is primary matching strategy; positional matching kept as fallback for ASR failures"
  - "MFA-aware gap threshold (0.4s) used after MFA refinement; preserves 0.8s default for backward compatibility"
  - "CRX fingerprint uses ErrorNumber:SentenceId pairs (simpler than full text hash, sufficient for invalidation)"

patterns-established:
  - "Auto-detect source type pattern: directory → folder import, file → session import"
  - "Filename error number extraction: direct (NNN), prefix (error_NNN), trailing digits fallback"

requirements-completed: [PFR-IMPORT, PFR-MATCH]

duration: 5min
completed: 2026-03-09
---

# Phase 15 Plan 03: Pickup Asset Import & Text-Similarity Matching Summary

**Unified PickupAssetService with disk caching plus greedy text-similarity matching and MFA-aware 0.4s utterance segmentation in PickupMatchingService**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-09T10:00:05Z
- **Completed:** 2026-03-09T10:05:25Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- PickupAssetService created with unified import API — auto-detects folder of individual WAVs vs single session file
- Individual file import matches by filename pattern (NNN, error_NNN, trailing digits) with ASR confidence scoring
- Session file import delegates to PickupMatchingService with disk cache invalidation on file change + CRX fingerprint
- MatchByTextSimilarity added to PickupMatchingService — greedy best-first assignment handles out-of-order recordings
- MFA-aware segmentation with configurable gap threshold (0.4s for MFA-refined timings, 0.8s default preserved)
- NormalizeForMatch and ExtractFullText promoted to public for cross-service reuse

## Task Commits

Each task was committed atomically:

1. **Task 1: PickupAssetService — unified import with cache** - `e4cf98f` (feat)
2. **Task 2: Enhanced PickupMatchingService — hybrid matching + MFA-aware segmentation** - `931875a` (feat)

## Files Created/Modified
- `host/Ams.Workstation.Server/Services/PickupAssetService.cs` - Unified pickup import with auto-detect, caching, and split matched/unmatched buckets
- `host/Ams.Workstation.Server/Services/PickupMatchingService.cs` - MatchByTextSimilarity, MFA-aware gap threshold, public NormalizeForMatch/ExtractFullText
- `host/Ams.Workstation.Server/Program.cs` - PickupAssetService transient DI registration

## Decisions Made
- **PickupAssetService as transient:** Follows PolishService pattern — stateless orchestrator injecting PickupMatchingService and BlazorWorkspace. No mutable state to protect.
- **Text similarity as primary, positional as fallback:** When ≥50% of segments have transcribed text, use MatchByTextSimilarity for robust out-of-order handling. Fall back to PairSegmentsToTargets when ASR fails.
- **MFA-aware gap threshold (0.4s):** MFA phone boundaries give more precise word endpoints, so shorter silences between them are legitimate utterance breaks. The 0.8s default is preserved via parameter default for backward compatibility.
- **CRX fingerprint simplification:** Uses `ErrorNumber:SentenceId` pairs joined by `;` then SHA256 — simpler than the full-text fingerprint in PolishService, sufficient for cache invalidation.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

Pre-existing build errors exist in PolishService.cs and StagingQueueService.cs/EditListService.cs (references to `ShiftDownstream`, `ChapterEdit`, `EditOperation`, `TimelineProjection`). These are from transitional work in prior plans (15-01/15-02 introduced new models that other files reference before their full integration in 15-04). Not caused by this plan's changes and not addressed here.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- PickupAssetService ready for integration into Polish page import workflow
- MatchByTextSimilarity ready for use by PickupAssetService and UI components
- Ready for Plan 15-04 (Refactored Apply/Revert Flow) which integrates these services into the full Polish pipeline

## Self-Check: PASSED

All created files verified on disk. All task commits verified in git history.

---
*Phase: 15-pickup-flow-redesign*
*Completed: 2026-03-09*
