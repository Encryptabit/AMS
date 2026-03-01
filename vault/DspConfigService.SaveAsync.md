---
namespace: "Ams.Cli.Services"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Services/DspConfigService.cs"
access_modifier: "internal"
complexity: 1
fan_in: 4
fan_out: 2
tags:
  - method
---
# DspConfigService::SaveAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Services/DspConfigService.cs`


#### [[DspConfigService.SaveAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static Task SaveAsync(DspConfig config, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[DspConfigService.GetConfigFilePath]]
- [[DspConfigService.NormalizeConfig]]

**Called-by <-**
- [[DspCommand.CreateInitCommand]]
- [[DspCommand.CreateSetDirAddCommand]]
- [[DspCommand.CreateSetDirClearCommand]]
- [[DspCommand.CreateSetDirRemoveCommand]]

