---
namespace: "Ams.Cli.Services"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Services/DspConfigService.cs"
access_modifier: "internal"
complexity: 2
fan_in: 11
fan_out: 2
tags:
  - method
  - danger/high-fan-in
  - llm/async
  - llm/data-access
  - llm/utility
  - llm/validation
---
# DspConfigService::LoadAsync
**Path**: `Projects/AMS/host/Ams.Cli/Services/DspConfigService.cs`

> [!danger] High Fan-In (11)
> This method is called by 11 other methods. Changes here have wide impact.

## Summary
**Asynchronously load DSP configuration data and return a normalized `DspConfig` for use across CLI commands.**

LoadAsync is an internal static async config bootstrap method that returns `Task<DspConfig>` with very low branching complexity (2). It resolves the configuration location via `GetConfigFilePath`, then normalizes the loaded configuration through `NormalizeConfig` before returning. The optional `CancellationToken` supports cooperative cancellation, and the method acts as a shared config-loading path for many command-construction methods.


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

