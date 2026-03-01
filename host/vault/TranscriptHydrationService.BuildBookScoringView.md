---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
---
# TranscriptHydrationService::BuildBookScoringView
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`

## Summary
**Creates a phoneme-aware scoring token view for a slice of book words used by hydration diff scoring.**

`BuildBookScoringView` builds a token/phoneme view for a bounded book-word range, returning `EmptyTokenPhonemeView` when the range is invalid. It clamps `end`, normalizes each word (`TextNormalizer.Normalize(..., expandContractions: true, removeNumbers: false)`), tokenizes to word tokens, and skips words that produce no tokens. For single-token words it resolves phoneme variants via `ResolveBookWordPhonemes(word, fallbackPronunciations)`; those variants are then associated with each emitted token for that source word. It returns a new `TokenPhonemeView(tokens, phonemes)` list pair aligned by index.


#### [[TranscriptHydrationService.BuildBookScoringView]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static TranscriptHydrationService.TokenPhonemeView BuildBookScoringView(BookIndex book, int start, int end, IReadOnlyDictionary<string, string[]> fallbackPronunciations)
```

**Calls ->**
- [[TextNormalizer.Normalize]]
- [[TextNormalizer.TokenizeWords]]
- [[TranscriptHydrationService.ResolveBookWordPhonemes]]

**Called-by <-**
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]

