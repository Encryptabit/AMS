---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 27
fan_in: 1
fan_out: 0
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
---
# TranscriptAligner::ComputeGuardRangeForMissingSentence
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

> [!danger] High Complexity (27)
> Cyclomatic complexity: 27. Consider refactoring into smaller methods.

## Summary
**Computes a best-effort ASR index range to constrain placement of a missing sentence in word-alignment space.**

`ComputeGuardRangeForMissingSentence` infers an ASR guard span for a sentence without direct matches by scanning `ops` for nearest neighboring mapped ASR indices (`prevAsr` before `sentenceStartWord`, `nextAsr` after `sentenceEndWord`). It then collects candidate ASR positions from in-sentence mapped ops and insertion ops (`BookIdx == null`) that lie strictly between those neighbors, and returns the sorted min/max candidate bounds when any exist. If no candidates are found, it falls back to neighbor-derived bounds (`prev+1..next-1` when possible) or a single-point guard adjacent to `prev`/`next`; if no anchors exist, it returns `(null, null)`.


#### [[TranscriptAligner.ComputeGuardRangeForMissingSentence]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (int? Start, int? End) ComputeGuardRangeForMissingSentence(IReadOnlyList<WordAlign> ops, int sentenceStartWord, int sentenceEndWord)
```

**Called-by <-**
- [[TranscriptAligner.Rollup_2]]

