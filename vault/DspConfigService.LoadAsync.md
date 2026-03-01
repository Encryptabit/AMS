---
namespace: "Ams.Cli.Services"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Services/DspConfigService.cs"
access_modifier: "internal"
complexity: 2
fan_in: 11
fan_out: 2
tags:
  - method
  - danger/high-fan-in
---
# DspConfigService::LoadAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Services/DspConfigService.cs`

> [!danger] High Fan-In (11)
> This method is called by 11 other methods. Changes here have wide impact.


#### [[DspConfigService.LoadAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static Task<DspConfig> LoadAsync(CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[DspConfigService.GetConfigFilePath]]
- [[DspConfigService.NormalizeConfig]]

**Called-by <-**
- [[DspCommand.CreateChainAddCommand]]
- [[DspCommand.CreateChainInsertCommand]]
- [[DspCommand.CreateChainListCommand]]
- [[DspCommand.CreateChainPrependCommand]]
- [[DspCommand.CreateInitCommand]]
- [[DspCommand.CreateListPluginsCommand]]
- [[DspCommand.CreateRunCommand]]
- [[DspCommand.CreateSetDirAddCommand]]
- [[DspCommand.CreateSetDirClearCommand]]
- [[DspCommand.CreateSetDirListCommand]]
- [[DspCommand.CreateSetDirRemoveCommand]]

