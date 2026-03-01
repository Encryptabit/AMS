---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "public"
complexity: 52
fan_in: 6
fan_out: 5
tags:
  - method
  - danger/high-complexity
  - llm/utility
---
# TranscriptAligner::Rollup
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

> [!danger] High Complexity (52)
> Cyclomatic complexity: 52. Consider refactoring into smaller methods.

## Summary
**Builds sentence and paragraph alignment summaries, including script ranges, quality metrics, and status labels, from word-level alignment operations and transcript boundaries.**

This overload converts `ops` into sentence- and paragraph-level alignment artifacts by scanning each book sentence range, selecting matching `WordAlign` items, and expanding boundaries to include adjacent insertion runs (`BookIdx == null`, `AsrIdx != null`). For each sentence it computes score-weighted WER (`costSum/tokenCount`), legacy WER, deletion/insertion counts, and CER via `ComputeCer`, while deriving `ScriptRange` from in-range/anchor ASR indices; if no candidates exist, it emits an `unreliable` sentence with empty timing and a guard range from `ComputeGuardRangeForMissingSentence`. When both `book` and `asr` are available, it compares normalized reference/hypothesis strings (`BuildNormalizedWordString`) and force-resets metrics to perfect match on exact normalized equality. After per-sentence pass it fills missing script ranges with `SynthesizeMissingScriptRanges`, then rolls up paragraph metrics by averaging sentence metrics and assigns paragraph status from WER thresholds.


#### [[TranscriptAligner.Rollup_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static (List<SentenceAlign> sents, List<ParagraphAlign> paras) Rollup(IReadOnlyList<WordAlign> ops, IReadOnlyList<(int Id, int Start, int End)> bookSentences, IReadOnlyList<(int Id, int Start, int End)> bookParagraphs, BookIndex book, AsrResponse asr)
```

**Calls ->**
- [[TranscriptAligner.BuildNormalizedWordString]]
- [[TranscriptAligner.BuildNormalizedWordString_2]]
- [[TranscriptAligner.ComputeCer]]
- [[TranscriptAligner.ComputeGuardRangeForMissingSentence]]
- [[TranscriptAligner.SynthesizeMissingScriptRanges]]

**Called-by <-**
- [[TranscriptAligner.Rollup]]
- [[TranscriptIndexService.BuildRollups]]
- [[TxAlignTests.Rollup_IgnoresInsertionsOutsideGuardSpan]]
- [[TxAlignTests.Rollup_IgnoresPurePunctuationDifferences]]
- [[TxAlignTests.Rollup_TreatsWhitespaceOnlyDifferencesAsMatches]]
- [[TxAlignTests.Rollup_UsesWeightedWerAndCer]]

