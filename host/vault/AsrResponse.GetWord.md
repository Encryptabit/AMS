---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrModels.cs"
access_modifier: "public"
complexity: 3
fan_in: 9
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# AsrResponse::GetWord
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrModels.cs`

## Summary
**Safely fetch a word at a given index from the ASR response, returning null when the index is invalid.**

`GetWord` is a bounds-checked accessor over the cached `Words` projection. It returns `Words[index]` only when `index >= 0 && index < WordCount`; otherwise it returns `null` instead of throwing `IndexOutOfRangeException`. This provides safe random access for downstream alignment/scoring pipelines that may probe uncertain indices.


#### [[AsrResponse.GetWord]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public string GetWord(int index)
```

**Called-by <-**
- [[AsrTranscriptBuilder.BuildAggregateText]]
- [[SentenceRefinementService.RefineAsync]]
- [[AnchorPreprocessor.BuildAsrView]]
- [[TranscriptAligner.BuildNormalizedWordString]]
- [[TranscriptHydrationService.BuildAsrScoringView]]
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]
- [[TranscriptHydrationService.JoinAsr]]
- [[TranscriptIndexService.BuildAsrPhonemeViewAsync]]
- [[ScriptValidator.ExtractWordsFromAsrResponse]]

