---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "private"
complexity: 12
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptIndexService::BuildWordOperations
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`

## Summary
**Builds canonical word alignment operations by merging n-gram anchor matches with windowed DP alignment results and projecting them to original token indices.**

This method composes final word-level alignment operations by combining anchor-derived matches with dynamic-programming window alignments. It first rebuilds the book token view, defines an equivalence map and filler-token set, then calls `TranscriptAligner.AlignWindows` over filtered token streams plus phoneme arrays. It expands each anchor across `policy.NGram`, maps filtered indices back to original book/ASR indices (`BookFilteredToOriginalWord`, `FilteredToOriginalToken`), emits deduplicated `"anchor"` `Match` ops, then converts DP ops to original indices and concatenates them. A final de-dup pass keyed by `(BookIdx, AsrIdx, Op)` yields `wordOps`, while returning `anchorOps` separately for downstream rollups.


#### [[TranscriptIndexService.BuildWordOperations]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (List<WordAlign> WordOps, List<WordAlign> AnchorOps) BuildWordOperations(AnchorPipelineResult pipeline, AnchorPolicy policy, BookIndex book, AsrAnchorView asrView, IReadOnlyList<(int bLo, int bHi, int aLo, int aHi)> windows, string[][] bookPhonemes, string[][] asrPhonemes)
```

**Calls ->**
- [[AnchorPreprocessor.BuildBookView]]
- [[TranscriptAligner.AlignWindows]]

**Called-by <-**
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]

