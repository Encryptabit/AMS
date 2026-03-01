---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 12
tags:
  - method
---
# TranscriptHydrationService::BuildHydratedTranscriptAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`


#### [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<HydratedTranscript> BuildHydratedTranscriptAsync(ChapterContext context, TranscriptIndex transcript, CancellationToken cancellationToken)
```

**Calls ->**
- [[AsrResponse.GetWord]]
- [[TextDiffAnalyzer.Analyze_2]]
- [[TranscriptHydrationService.BuildAsrScoringView]]
- [[TranscriptHydrationService.BuildBookScoringView]]
- [[TranscriptHydrationService.BuildParagraphScoringView]]
- [[TranscriptHydrationService.BuildParagraphScript]]
- [[TranscriptHydrationService.BuildPhonemeAwareScoringOptions]]
- [[TranscriptHydrationService.BuildPronunciationFallbackAsync]]
- [[TranscriptHydrationService.JoinAsr]]
- [[TranscriptHydrationService.JoinBook]]
- [[TranscriptHydrationService.ResolveParagraphStatus]]
- [[TranscriptHydrationService.ResolveSentenceStatus]]

**Called-by <-**
- [[TranscriptHydrationService.HydrateTranscriptAsync]]

