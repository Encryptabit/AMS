---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 10
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/async
  - llm/utility
  - llm/di
  - llm/validation
  - llm/error-handling
  - llm/data-access
---
# TranscriptHydrationService::BuildPronunciationFallbackAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`

## Summary
**Asynchronously resolves fallback pronunciation variants for ASR/book tokens needed by phoneme-aware hydration comparisons.**

`BuildPronunciationFallbackAsync` constructs a fallback lexeme-to-phoneme map used during hydrate scoring when canonical phonemes are missing. It collects unique normalized lexemes from all ASR words and from transcript-referenced book words that lack phonemes, using bounded range checks for sentence book spans. If no lexemes are collected it returns an empty dictionary immediately; otherwise it calls `_pronunciationProvider.GetPronunciationsAsync(...)`. Non-cancellation lookup failures are caught, logged, and downgraded to an empty dictionary so hydration can proceed without phoneme augmentation.


#### [[TranscriptHydrationService.BuildPronunciationFallbackAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<IReadOnlyDictionary<string, string[]>> BuildPronunciationFallbackAsync(BookIndex book, AsrResponse asr, TranscriptIndex transcript, CancellationToken cancellationToken)
```

**Calls ->**
- [[Log.Debug]]
- [[IPronunciationProvider.GetPronunciationsAsync]]
- [[PronunciationHelper.NormalizeForLookup]]

**Called-by <-**
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]

