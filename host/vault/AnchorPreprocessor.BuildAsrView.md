---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPreprocessor.cs"
access_modifier: "public"
complexity: 5
fan_in: 3
fan_out: 2
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# AnchorPreprocessor::BuildAsrView
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/AnchorPreprocessor.cs`

## Summary
**Builds a cleaned ASR token view and index map from filtered token positions back to original ASR word indices.**

BuildAsrView projects ASR words into a normalized token stream with filtered-to-original index mapping for alignment. It short-circuits to empty arrays when `asr.HasWords` is false, otherwise iterates `0..asr.WordCount-1`, fetches each token via `asr.GetWord(i)`, skips null/whitespace entries, normalizes with `AnchorTokenizer.Normalize`, and skips empty normalized results. For retained tokens it appends normalized text to `tokens` and original ASR indices to `filteredToOriginal`, then returns `new AsrAnchorView(tokens, filteredToOriginal)`.


#### [[AnchorPreprocessor.BuildAsrView]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AsrAnchorView BuildAsrView(AsrResponse asr)
```

**Calls ->**
- [[AsrResponse.GetWord]]
- [[AnchorTokenizer.Normalize]]

**Called-by <-**
- [[AnchorPipeline.ComputeAnchors]]
- [[AnchorComputeService.ComputeAnchorsAsync]]
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]

