---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 12
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
---
# TranscriptHydrationService::BuildAsrScoringView
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`

## Summary
**Builds a phoneme-aware token scoring view from ASR words for hydration diff analysis.**

`BuildAsrScoringView` produces a token/phoneme scoring view for an ASR word range with nullable bounds handling. It returns `EmptyTokenPhonemeView` when bounds are missing/invalid, otherwise clamps indices to ASR limits, normalizes and tokenizes each ASR word, and skips empty/whitespace results. For single-token words it resolves pronunciation variants from the provided lookup map using `PronunciationHelper.NormalizeForLookup(word)`; those variants are associated with each emitted token from that source word. The method returns aligned token and phoneme-variant lists as `TokenPhonemeView`.


#### [[TranscriptHydrationService.BuildAsrScoringView]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static TranscriptHydrationService.TokenPhonemeView BuildAsrScoringView(AsrResponse asr, int? start, int? end, IReadOnlyDictionary<string, string[]> pronunciations)
```

**Calls ->**
- [[AsrResponse.GetWord]]
- [[TextNormalizer.Normalize]]
- [[TextNormalizer.TokenizeWords]]
- [[PronunciationHelper.NormalizeForLookup]]

**Called-by <-**
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]

