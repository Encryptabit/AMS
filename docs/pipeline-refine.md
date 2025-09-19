# Sentence Refinement Stage

The refine stage now reuses artifacts produced earlier in the pipeline instead of invoking long-running
external tools. During `SentenceRefinementStage` execution:

- `align-chunks/chunks/*.aeneas.json` is loaded and projected into chapter time to seed sentence start/end.
- `timeline/silence.json` contributes silence spans; when available the first silence after a sentence is used
  to snap the tail within a configurable guard window.
- `transcripts/asr.json` provides the ASR tokens that anchor each sentence and supply fallbacks when fragments
  are missing.

The stage builds a `SentenceRefinementContext` with fragment timings, ordered silences, and tail guards before
handing control to `SentenceRefinementService`. The service enforces monotonic sentence boundaries, keeps
fragments deterministic, and falls back to token timing whenever fragment data is absent. Operators can tune
`SentenceRefinementParams.MinTailSec` and `SentenceRefinementParams.MaxSnapAheadSec` when slower or more
aggressive snapping is required.

## Diagnostics

Sentence refinement logs the number of fragment-backed sentences versus fallbacks and the total silence events
considered. These counters show up in the stage output and can be used to spot regressions when chunk alignment
artifacts go missing.

### CLI Usage

`Ams.Cli refine-sentences` now consumes the same artifacts as the pipeline stage. Supply `--work` (or explicit `--book-index`, `--anchors-json`, `--alignments-dir`, `--silence-json`) so the command can reuse per-chunk fragments and silences.

### Output Notes

- `sentences.json.text` contains the actual book text for each scoped sentence rather than ``Sentence N``.
- `sentences.json.conf` reflects whether a fragment backed the sentence (`1.0`) or it fell back to ASR timing (`0.6`).
