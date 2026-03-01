---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 2
tags:
  - method
---
# TranscriptHydrationService::JoinAsr
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`


#### [[TranscriptHydrationService.JoinAsr]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string JoinAsr(AsrResponse asr, int? start, int? end)
```

**Calls ->**
- [[AsrResponse.GetWord]]
- [[TranscriptHydrationService.NormalizeSurface]]

**Called-by <-**
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]

