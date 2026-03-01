---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 38
fan_in: 1
fan_out: 3
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
---
# TranscriptAligner::SynthesizeMissingScriptRanges
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

> [!danger] High Complexity (38)
> Cyclomatic complexity: 38. Consider refactoring into smaller methods.

## Summary
**Infers and assigns non-overlapping ASR token ranges for sentences that are missing script ranges using guard hints and neighboring aligned sentences.**

`SynthesizeMissingScriptRanges` backfills `SentenceAlign.ScriptRange` for sentences lacking concrete ranges when ASR tokens exist, first collecting missing indices via `TryGetConcreteRange` and processing contiguous missing blocks. For each block it derives allocation bounds from block-level guard min/max (`guardRanges`) or neighboring concrete ranges (`FindPreviousRange`/`FindNextRange`), clamps to `[0, asrTokenCount-1]`, and falls back to a single anchor index when bounds invert. It then partitions the available span across block sentences using proportional floor-based slicing, with post-adjustments to avoid overlap with already concrete previous/next sentence ranges. Each missing sentence is rewritten with a synthesized `ScriptRange(startIdx, endIdx)`.


#### [[TranscriptAligner.SynthesizeMissingScriptRanges]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void SynthesizeMissingScriptRanges(List<SentenceAlign> sentences, int asrTokenCount, (int? Start, int? End)[] guardRanges)
```

**Calls ->**
- [[TranscriptAligner.FindNextRange]]
- [[TranscriptAligner.FindPreviousRange]]
- [[TranscriptAligner.TryGetConcreteRange]]

**Called-by <-**
- [[TranscriptAligner.Rollup_2]]

