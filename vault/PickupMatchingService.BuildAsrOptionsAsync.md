---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
---
# PickupMatchingService::BuildAsrOptionsAsync
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs`


#### [[PickupMatchingService.BuildAsrOptionsAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<AsrOptions> BuildAsrOptionsAsync(CancellationToken ct)
```

**Calls ->**
- [[AsrEngineConfig.ResolveModelPathAsync]]

**Called-by <-**
- [[PickupMatchingService.MatchPickupCrxAsync]]
- [[PickupMatchingService.MatchSinglePickupAsync]]

