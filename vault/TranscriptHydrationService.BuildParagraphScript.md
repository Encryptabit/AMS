---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
---
# TranscriptHydrationService::BuildParagraphScript
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`


#### [[TranscriptHydrationService.BuildParagraphScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildParagraphScript(IReadOnlyList<int> sentenceIds, IReadOnlyDictionary<int, HydratedSentence> sentenceMap)
```

**Called-by <-**
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]

