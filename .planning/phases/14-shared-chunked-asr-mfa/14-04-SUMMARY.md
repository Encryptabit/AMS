---
phase: 14-shared-chunked-asr-mfa
plan: 04
subsystem: alignment
tags: [mfa, chunking, corpus, forced-alignment, audio-slicing]

# Dependency graph
requires:
  - phase: 14-01
    provides: ChunkPlanDocument and ChunkPlanEntry models
  - phase: 14-02
    provides: ChunkPlanningService with deterministic silence-based boundary detection
provides:
  - MfaChunkCorpusBuilder for per-chunk wav/lab corpus generation from shared plan
  - Chunked corpus path in MfaWorkflow with automatic fallback to legacy single-utterance
  - Per-utterance TextGrid collection for chunked alignment output
affects: [14-05, 14-06, 14-07]

# Tech tracking
tech-stack:
  added: []
  patterns: [chunk-to-sentence timing overlap mapping, nearest-sentence fallback for sparse timing windows]

key-files:
  created:
    - host/Ams.Core/Application/Mfa/MfaChunkCorpusBuilder.cs
    - host/Ams.Tests/Application/Mfa/MfaChunkCorpusBuilderTests.cs
  modified:
    - host/Ams.Core/Application/Mfa/MfaWorkflow.cs

key-decisions:
  - "Chunk corpus only activates when chunk plan has >1 chunks; single-chunk plans use legacy path"
  - "Chunked MFA path omits ASR corpus fallback (only applies to legacy single-utterance)"
  - "MinLabTokenCount=2 ensures chunks with very short text (single word) are skipped"
  - "Nearest-sentence fallback uses timing midpoint proximity; untimed transcripts use proportional index"

patterns-established:
  - "Chunk-to-text mapping: interval overlap [a,b) intersects [c,d) iff a<d && c<b"
  - "Utterance naming: utt-NNNN (4-digit zero-padded) for deterministic ordering"
  - "Corpus directory cleaned before chunk build to prevent stale artifacts"

requirements-completed: [CHUNK-MFA]

# Metrics
duration: 4min
completed: 2026-03-05
---

# Phase 14 Plan 04: Chunked MFA Corpus Summary

**MfaChunkCorpusBuilder generates per-chunk wav/lab corpus from shared chunk plan with BookText-driven labs and nearest-sentence fallback for sparse timing windows**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-05T06:26:06Z
- **Completed:** 2026-03-05T06:30:54Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- MfaChunkCorpusBuilder slices chapter audio into per-chunk wav files using AudioBuffer.Slice + AudioProcessor.EncodeWav
- Lab files derived exclusively from HydratedSentence.BookText via PronunciationHelper normalization (never ASR words)
- MfaWorkflow automatically branches to chunked corpus when chunk plan has multiple chunks, preserving legacy path
- 17 unit tests cover overlap mapping, sparse fallback, boundary conditions, and deterministic output

## Task Commits

Each task was committed atomically:

1. **Task 1: Build chunked MFA corpus from shared plan** - `334c7a2` (feat)
2. **Task 2: Add robust chunk-text fallback behavior** - `a60d33b` (feat)

## Files Created/Modified
- `host/Ams.Core/Application/Mfa/MfaChunkCorpusBuilder.cs` - Per-chunk corpus builder with overlap mapping and fallback
- `host/Ams.Core/Application/Mfa/MfaWorkflow.cs` - Chunked/legacy branching, clean corpus directory, per-utterance TextGrid collection
- `host/Ams.Tests/Application/Mfa/MfaChunkCorpusBuilderTests.cs` - 17 tests for corpus builder logic

## Decisions Made
- Chunk corpus only activates when chunk plan has >1 chunks to avoid unnecessary overhead for single-chunk chapters
- Chunked MFA path skips the ASR corpus fallback retry loop (only relevant for legacy single-utterance where the whole-chapter lab might fail)
- MinLabTokenCount of 2 prevents empty/trivial lab files that would cause MFA alignment errors
- Nearest-sentence fallback computes timing midpoint proximity to chunk midpoint; for fully untimed transcripts, uses proportional index mapping

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Chunked MFA corpus builder is ready for integration with the full pipeline
- Per-utterance TextGrids are collected in the mfa copy directory for downstream timing merge
- TextGrid aggregation (combining per-chunk TextGrids back to chapter-level timings) will be needed in a subsequent plan

---
*Phase: 14-shared-chunked-asr-mfa*
*Completed: 2026-03-05*
