---
phase: quick
plan: 001
type: execute
---

<objective>
Stabilize and simplify the manuscript/transcript error pipeline so reviewer-facing diffs stop generating synthetic artifacts (word doubling, token hybrids, boundary bleed, wrong contraction expansions).

Purpose: Reduce false app bugs by removing fragile heuristics and tightening sentence/token ownership.
Output: Deterministic diff rendering + safer core diff ops + improved sentence boundary mapping.
</objective>

<context>
@/mnt/c/Projects/error-review.md
@/mnt/c/Projects/error-review-continuation.md
@host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs
@host/Ams.Core/Common/TextNormalizer.cs
@host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs
@tools/validation-viewer/static/app.js
@tools/validation-viewer/server.py

Observed patterns to eliminate:
- split/join hybrids (`bots hackerbots`, `team fireteam`, `strike airstrike`)
- synthetic substitution/reorder in UI diff rendering
- boundary bleed from neighboring sentence words
- ambiguous contraction expansion artifacts (`he'd` -> `he would`, `I'd` -> `I would`)
</context>

<tasks>
<task type="auto">
  <name>Task 1: Simplify viewer diff rendering to preserve op order</name>
  <files>tools/validation-viewer/static/app.js</files>
  <action>Replace global fuzzy token pairing in `renderUnifiedDiff()` with deterministic sequential rendering from `diff.ops`. Only allow local pairing when delete and insert blocks are adjacent and index-aligned; otherwise render raw delete/insert blocks in original order. Keep CRX/comment generation consistent with this deterministic order.</action>
  <verify>Open a chapter with known repeated tokens and confirm no synthetic reorder/doubling in rendered diff cards.</verify>
  <done>UI no longer invents substitutions across distant tokens; displayed diffs reflect backend op sequence exactly.</done>
</task>

<task type="auto">
  <name>Task 2: Remove fragile glue heuristics from core diff output</name>
  <files>host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs, host/Ams.Core/Common/TextNormalizer.cs</files>
  <action>Simplify `TextDiffAnalyzer` by removing or strictly constraining glue-token postprocessing so it cannot partially consume delete spans. Separate scoring normalization from display tokens: avoid forcing ambiguous contraction expansions into reviewer-facing diff text (`'d` forms). Keep metrics behavior explicit and deterministic.</action>
  <verify>dotnet build host/Ams.Core/Ams.Core.csproj</verify>
  <done>No hybrid tokens are emitted by core diff ops; ambiguous contractions no longer produce semantically wrong reviewer text.</done>
</task>

<task type="auto">
  <name>Task 3: Tighten sentence ownership for insertions and range expansion</name>
  <files>host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs, tools/validation-viewer/server.py</files>
  <action>Reduce cross-sentence bleed by limiting insertion-based script-range expansion in `TranscriptAligner`. In `server.py`, replace `last_sentence_id` fallback assignment with sentence selection derived from `asrIdx` and sentence `scriptRange` bounds (or nearest bounded sentence when unavoidable). Preserve chronological ordering and avoid attaching stray inserts to prior sentence by default.</action>
  <verify>python3 -m py_compile tools/validation-viewer/server.py && dotnet build host/Ams.Core/Ams.Core.csproj</verify>
  <done>Adjacent-sentence words no longer appear in the wrong sentence diff unless source alignment itself is ambiguous.</done>
</task>
</tasks>

<verification>
- Build passes for Ams.Core after core alignment/diff changes.
- Validation viewer server compiles and runs.
- Manual spot-check on previously flagged chapters confirms reduction of synthetic app-bug classes.
</verification>

<success_criteria>
- [ ] Deterministic diff rendering (no global fuzzy pairing artifacts)
- [ ] Core diff ops do not emit split/join hybrids from glue logic
- [ ] Sentence-level bleed is materially reduced in known problem chapters
- [ ] Reviewer output reflects real ASR/book differences, not transformation artifacts
</success_criteria>

<output>
On execution completion, create:
- `.planning/quick/001-simplify-alignment-diff-pipeline-errors/001-SUMMARY.md`

Include:
- changed files
- before/after behavior by error class
- residual risks and follow-up items
</output>
