---
phase: 14-shared-chunked-asr-mfa
plan: 05
subsystem: alignment
tags: [mfa, textgrid, chunked-alignment, praat, aggregation]

requires:
  - phase: 14-04
    provides: MfaChunkCorpusBuilder with per-chunk wav/lab corpus and UtteranceEntry metadata
provides:
  - TextGridAggregationService for merging per-chunk TextGrids into chapter-level TextGrid
  - MfaWorkflow integration invoking aggregation after chunked alignment
  - 8 unit tests covering offset correctness, monotonic ordering, and parser compatibility
affects: [14-06, 14-07, MergeTimingsCommand, PipelineService]

tech-stack:
  added: []
  patterns: [chunk-offset-aggregation, praat-textgrid-generation]

key-files:
  created:
    - host/Ams.Core/Application/Mfa/TextGridAggregationService.cs
    - host/Ams.Tests/Application/Mfa/TextGridAggregationServiceTests.cs
  modified:
    - host/Ams.Core/Application/Mfa/MfaWorkflow.cs

key-decisions:
  - "Aggregate into standard Praat full-text TextGrid format for compatibility with TextGridParser"
  - "Include both words and phones tiers in aggregated output for downstream phone-level features"
  - "Sort intervals by start time after offset application for monotonic ordering guarantee"

patterns-established:
  - "TextGrid generation: Use StringBuilder with CultureInfo.InvariantCulture for Praat-compatible floating point formatting"

requirements-completed: [CHUNK-TEXTGRID]

duration: 4min
completed: 2026-03-05
---

# Phase 14 Plan 05: TextGrid Aggregation Summary

**Chunk-level MFA TextGrid aggregation service with offset-correct merging into canonical chapter-level TextGrid, preserving MergeTimingsCommand contract**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-05T06:34:11Z
- **Completed:** 2026-03-05T06:38:17Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- TextGridAggregationService aggregates per-chunk MFA TextGrids with correct time offset application
- MfaWorkflow now invokes aggregation after collecting per-utterance TextGrids in chunked mode
- 8 unit tests validate offset correctness, monotonic ordering, missing/empty chunk handling, round-trip parser compatibility, and single-application offset guard

## Task Commits

Each task was committed atomically:

1. **Task 1: Build chunk TextGrid aggregation service** - `7b3f3e1` (feat)
2. **Task 2: Integrate aggregation into MFA workflow** - `fc965c9` (feat)

## Files Created/Modified
- `host/Ams.Core/Application/Mfa/TextGridAggregationService.cs` - Aggregates per-chunk TextGrids into canonical chapter-level TextGrid with offset application and monotonic ordering
- `host/Ams.Core/Application/Mfa/MfaWorkflow.cs` - Replaced chapter-level TextGrid copy attempt with aggregation call in chunked path
- `host/Ams.Tests/Application/Mfa/TextGridAggregationServiceTests.cs` - 8 tests covering offset correctness, ordering, empty handling, parser round-trip

## Decisions Made
- Used standard Praat full-text TextGrid format for maximum compatibility with existing TextGridParser
- Include both words and phones tiers in aggregated output to support future phone-level features
- Sort intervals by start time after offset application for guaranteed monotonic ordering
- Aggregate reads from mfaCopyDir (post-collection) rather than directly from MFA output dirs for consistent file resolution

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Chunked MFA pipeline now produces canonical chapter-level TextGrid compatible with MergeTimingsCommand
- MergeTimingsCommand requires no changes - its single-TextGrid-input contract is preserved
- Ready for plan 14-06 (remaining integration/pipeline work)

---
*Phase: 14-shared-chunked-asr-mfa*
*Completed: 2026-03-05*
