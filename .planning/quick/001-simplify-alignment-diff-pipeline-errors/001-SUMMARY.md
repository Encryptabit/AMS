# Quick Plan 001: Simplify Alignment/Diff Pipeline Errors Summary

**Deterministic op-order diff rendering, contraction-safe reviewer tokens, and stricter sentence range ownership to reduce synthetic transcript artifacts**

## Performance

- **Duration:** 24 min
- **Started:** 2026-02-17T01:33:00Z
- **Completed:** 2026-02-17T01:57:00Z
- **Tasks:** 3
- **Files modified:** 5

## Accomplishments
- Replaced global fuzzy diff token pairing in the validation viewer with deterministic sequential rendering from backend op order.
- Removed glue-token postprocessing from `TextDiffAnalyzer` and split normalization paths so reviewer-facing diff tokens no longer force ambiguous contraction expansion.
- Tightened sentence script-range ownership in `TranscriptAligner` and replaced `last_sentence_id` fallback grouping in viewer server parsing with `asrIdx` + `scriptRange` based sentence selection.

## Files Created/Modified
- `.planning/quick/001-simplify-alignment-diff-pipeline-errors/001-SUMMARY.md` - Execution summary for quick plan 001.
- `tools/validation-viewer/static/app.js` - Deterministic `renderUnifiedDiff()` implementation with local adjacent delete→insert index pairing only.
- `host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs` - Removed glue heuristics; separated scoring normalization from display-token normalization.
- `host/Ams.Core/Common/TextNormalizer.cs` - Preserved apostrophes during punctuation normalization.
- `host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs` - Removed broad insertion-run script-range expansion to reduce cross-sentence bleed.
- `tools/validation-viewer/server.py` - Replaced fallback sentence assignment with sentence selection via `bookIdx` map first, then `asrIdx` against sentence `scriptRange` (nearest bounded fallback).

## Before/After by Error Class

| Error Class | Before | After |
|---|---|---|
| Synthetic substitution/reorder in UI | Global fuzzy matching paired distant tokens and could invent substitutions/reordering. | Rendering follows `diff.ops` strictly; only adjacent delete→insert blocks are locally index-paired. |
| Split/join hybrids (`bots hackerbots`, `strike airstrike`) | Glue postprocessing could partially consume delete spans and emit hybrids. | Glue postprocessing removed; diff ops are emitted directly from token diff without hybrid-merge heuristics. |
| Ambiguous contraction artifacts (`he'd` -> `he would`) | Reviewer-facing tokens inherited contraction expansion from normalization. | Display normalization keeps contractions unexpanded; apostrophes are preserved in normalized tokens. |
| Boundary bleed across neighboring sentences | Script ranges were expanded through insertion runs, potentially crossing sentence ownership. | Script range uses strict base sentence ownership; insertion-run extension removed. |
| Orphan word-op sentence assignment in viewer parsing | Missing `bookIdx` fell back to `last_sentence_id`, causing prior-sentence attachment bias. | Missing `bookIdx` now uses `asrIdx` against sentence `scriptRange`, with nearest bounded fallback when needed. |

## Decisions Made
- Removed glue-token postprocessing entirely instead of trying to further tune it, prioritizing deterministic correctness over heuristic merge behavior.
- Decoupled scoring and display normalization in `TextDiffAnalyzer` so metric behavior can remain normalized while reviewer text stays faithful.
- Enforced strict sentence script-range ownership in alignment to reduce false bleed, accepting narrower ranges over aggressive insertion capture.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
- Manual UI verification on flagged chapters was not run in this execution session. Automated compile/build verification completed successfully.

## Verification
- `dotnet build host/Ams.Core/Ams.Core.csproj` ✅
- `python3 -m py_compile tools/validation-viewer/server.py` ✅
- `node --check tools/validation-viewer/static/app.js` ✅

## Residual Risks and Follow-up
- Viewer `parse_report()` fallback path is now safer for sentence assignment, but primary runtime path uses hydrate-native sentence diffs; full chapter spot-checks should confirm no regressions in chapter cards and CRX workflows.
- Strict script-range ownership may under-capture legitimate edge insertions in some ambiguous alignments; if observed, add a constrained same-sentence insertion expansion rule with explicit boundary checks.
- Existing CRX comment formatting remains op-order deterministic, but optional future work is to share a single diff linearization helper across render/comment code paths for stricter consistency guarantees.

## Next Phase Readiness
- Quick-plan objective is complete and verified at compile/build level.
- Main roadmap remains ready for `10-03` (`Error Patterns Aggregation`) with this defect-reduction patch set available for validation workflows.

---
*Phase: quick*
*Completed: 2026-02-17*
