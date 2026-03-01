---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 10
fan_in: 1
fan_out: 3
tags:
  - method
---
# TranscriptHydrationService::BuildPronunciationFallbackAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`


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

