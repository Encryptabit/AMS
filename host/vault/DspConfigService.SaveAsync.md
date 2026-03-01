---
namespace: "Ams.Cli.Services"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Services/DspConfigService.cs"
access_modifier: "internal"
complexity: 1
fan_in: 4
fan_out: 2
tags:
  - method
  - llm/data-access
  - llm/async
  - llm/utility
  - llm/validation
---
# DspConfigService::SaveAsync
**Path**: `Projects/AMS/host/Ams.Cli/Services/DspConfigService.cs`

## Summary
**Save the DSP CLI configuration to the canonical config file after normalizing it.**

`SaveAsync` is a low-complexity (`CC=1`) async orchestration method in `Ams.Cli.Services.DspConfigService` that resolves the config file location via `GetConfigFilePath`, normalizes the incoming `DspConfig` using `NormalizeConfig`, and writes the normalized config with cancellation support. It is the shared persistence path used by `CreateInitCommand`, `CreateSetDirAddCommand`, `CreateSetDirClearCommand`, and `CreateSetDirRemoveCommand`.


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

