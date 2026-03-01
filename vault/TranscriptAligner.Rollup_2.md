---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "public"
complexity: 52
fan_in: 6
fan_out: 5
tags:
  - method
  - danger/high-complexity
---
# TranscriptAligner::Rollup
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

> [!danger] High Complexity (52)
> Cyclomatic complexity: 52. Consider refactoring into smaller methods.


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

