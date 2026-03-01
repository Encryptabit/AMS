---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
---
# TranscriptIndexService::BuildFallbackWindows
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`


#### [[TranscriptIndexService.BuildFallbackWindows]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<(int bLo, int bHi, int aLo, int aHi)> BuildFallbackWindows(AnchorPipelineResult pipeline, int asrTokenCount, AnchorPolicy policy)
```

**Called-by <-**
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]

