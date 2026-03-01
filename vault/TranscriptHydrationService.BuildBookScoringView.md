---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 3
tags:
  - method
---
# TranscriptHydrationService::BuildBookScoringView
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`


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

