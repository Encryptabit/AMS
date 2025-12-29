# Phase 2 Plan 3: Indexing Clarification Summary

**Book indexing creates static word/section structure from markdown (once); ASR indexing builds filtered token views for alignment (per-stage). PIPELINE-FLOW.md synthesizes all Phase 2 findings as definitive reference.**

## Performance

- **Duration:** 3 min
- **Started:** 2025-12-28T21:20:57Z
- **Completed:** 2025-12-28T21:24:22Z
- **Tasks:** 3
- **Files created:** 2

## Accomplishments

- Clear distinction between book indexing (Stage 1) and ASR response indexing (Stages 3-4)
- Complete timeline showing when each indexing process runs
- PIPELINE-FLOW.md as single source of truth for pipeline understanding
- All PROJECT.md open questions answered

## Files Created

- `.planning/phases/02-pipeline-analysis/INDEXING.md` - 250+ lines
- `.planning/phases/02-pipeline-analysis/PIPELINE-FLOW.md` - 400+ lines

## Key Findings

### Book Indexing

- **When:** Stage 1, before any chapter processing
- **What:** `BookIndexer.CreateIndexAsync()` parses markdown
- **Output:** `BookIndex` with `Words[]`, `Sentences[]`, `Sections[]`
- **Persistence:** Saved to `book-index.json`, reused for all chapters
- **Purpose:** Enables positional lookup and chapter-level processing

### ASR Response Indexing

- **When:** Stages 3-4, inside alignment operations
- **What:** `AnchorPreprocessor.BuildAsrView()` filters tokens
- **Output:** `AsrAnchorView` with `FilteredToOriginalToken[]` mapping
- **Persistence:** In-memory only, discarded after alignment
- **Purpose:** Enables stopword filtering and position recovery

### Relationship

Both create parallel structures for alignment:
- Book: `Words[] → BookView.Tokens[] → FilteredToOriginalWord[]`
- ASR: `Tokens[] → AsrView.Tokens[] → FilteredToOriginalToken[]`

Anchor matching works on filtered views, then recovers original positions.

## Phase 2 Complete

Phase 2 Pipeline Analysis is **complete**. All 3 plans executed:

| Plan | Focus | Key Output |
|------|-------|------------|
| 02-01 | Orchestration | PIPELINE-ORCHESTRATION.md |
| 02-02 | Data Flow | DATA-FLOW.md, ARTIFACTS.md |
| 02-03 | Indexing | INDEXING.md, PIPELINE-FLOW.md |

Ready for Phase 3 (Code Audit).

## Open Questions Answered

### Q: What is the exact pipeline step order?

**A:** 7 steps in fixed order:
1. BookIndex → 2. ASR → 3. Anchors → 4. Transcript → 5. Hydrate → 6. MFA → 7. Merge

### Q: What is book indexing vs ASR indexing?

**A:**
- **Book Indexing**: One-time parsing of book markdown → `BookIndex` → saved to JSON
- **ASR Indexing**: Per-alignment preprocessing → `AsrAnchorView` → in-memory only

They are different operations at different stages, not variations of the same thing.

---
*Phase: 02-pipeline-analysis*
*Completed: 2025-12-28*
