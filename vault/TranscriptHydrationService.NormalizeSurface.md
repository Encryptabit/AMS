---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 1
tags:
  - method
---
# TranscriptHydrationService::NormalizeSurface
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`


#### [[TranscriptHydrationService.NormalizeSurface]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeSurface(string text)
```

**Calls ->**
- [[TextNormalizer.NormalizeTypography]]

**Called-by <-**
- [[TranscriptHydrationService.JoinAsr]]
- [[TranscriptHydrationService.JoinBook]]

