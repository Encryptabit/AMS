---
phase: 14-shared-chunked-asr-mfa
plan: 01
subsystem: alignment
tags: [chunk-plan, artifact, document-slot, resolver, asr, mfa]

# Dependency graph
requires: []
provides:
  - ChunkPlanDocument artifact model (ChunkPlanDocument, ChunkPlanPolicy, ChunkPlanEntry)
  - IArtifactResolver chunk plan Load/Save/GetFile methods
  - ChapterDocuments.ChunkPlan DocumentSlot with full lifecycle participation
affects: [14-02, 14-03, 14-04, 14-05, 14-06, 14-07]

# Tech tracking
tech-stack:
  added: []
  patterns: [DocumentSlot artifact lifecycle for chunk plan]

key-files:
  created:
    - host/Ams.Core/Artifacts/Alignment/ChunkPlanDocument.cs
  modified:
    - host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs
    - host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs
    - host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs

key-decisions:
  - "Chunk plan persisted as {chapterStem}.align.chunks.json following existing artifact naming convention"
  - "ChunkPlanPolicy captures silence threshold, min silence/chunk durations, and sample rate for plan validity checking"
  - "ChunkPlanEntry includes both sample-precise (StartSample, LengthSamples) and time-domain (StartSec, EndSec) fields"

patterns-established:
  - "Chunk plan follows same DocumentSlot + resolver architecture as transcript/hydrate/anchors/asr"

requirements-completed: [CHUNK-ARTIFACTS]

# Metrics
duration: 2min
completed: 2026-03-05
---

# Phase 14 Plan 01: Chunk Plan Artifact Summary

**First-class ChunkPlanDocument artifact with DocumentSlot lifecycle, resolver persistence as align.chunks.json, shared by ASR and MFA stages**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-05T06:17:16Z
- **Completed:** 2026-03-05T06:19:27Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- Defined canonical ChunkPlanDocument with version, audio identity fingerprint, chunking policy metadata, and ordered chunk entries
- Wired chunk plan through IArtifactResolver and FileArtifactResolver as align.chunks.json
- Integrated DocumentSlot<ChunkPlanDocument> into ChapterDocuments with IsDirty and SaveChanges participation

## Task Commits

Each task was committed atomically:

1. **Task 1: Define chunk plan artifact model** - `840b7d6` (feat)
2. **Task 2: Wire chunk artifact through resolver + chapter documents** - `54781eb` (feat)

## Files Created/Modified
- `host/Ams.Core/Artifacts/Alignment/ChunkPlanDocument.cs` - ChunkPlanDocument, ChunkPlanPolicy, ChunkPlanEntry record types
- `host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs` - LoadChunkPlan, SaveChunkPlan, GetChunkPlanFile interface methods
- `host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs` - Implementation using LoadJson/SaveJson with align.chunks.json suffix
- `host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs` - DocumentSlot<ChunkPlanDocument> with property, IsDirty, SaveChanges, backing file getter

## Decisions Made
- Chunk plan artifact named `{chapterStem}.align.chunks.json` to align with existing `align.tx.json`, `align.hydrate.json`, `align.anchors.json` convention
- ChunkPlanPolicy records the exact parameters used for chunking (silence threshold dB, min silence duration ms, min chunk duration sec, sample rate) so downstream stages can validate plan freshness
- ChunkPlanEntry stores both sample-precise fields (StartSample, LengthSamples) for audio slicing and time-domain fields (StartSec, EndSec) for alignment offset math
- Document version field (currently 1) enables future schema evolution without breaking existing artifacts

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- ChunkPlanDocument is ready for plan 14-02+ to produce and consume through ChapterDocuments.ChunkPlan
- All downstream plans can access chunk plan via `chapter.Documents.ChunkPlan` without ad-hoc file IO
- Backing file resolves through resolver as `{chapterStem}.align.chunks.json`

## Self-Check: PASSED

All created files verified on disk. All commit hashes found in git log.

---
*Phase: 14-shared-chunked-asr-mfa*
*Completed: 2026-03-05*
