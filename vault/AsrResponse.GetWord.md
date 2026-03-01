---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Asr/AsrModels.cs"
access_modifier: "public"
complexity: 3
fan_in: 9
fan_out: 0
tags:
  - method
---
# AsrResponse::GetWord
**Path**: `home/cari/repos/AMS/host/Ams.Core/Asr/AsrModels.cs`


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

