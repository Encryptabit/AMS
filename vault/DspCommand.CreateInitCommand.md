---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 8
tags:
  - method
---
# DspCommand::CreateInitCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.CreateInitCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateInitCommand()
```

**Calls ->**
- [[DspCommand.ExtractPluginName]]
- [[DspCommand.ParseParameterLines]]
- [[DspConfigService.LoadAsync]]
- [[DspConfigService.SaveAsync]]
- [[PlugalyzerService.RunAsync]]
- [[Log.Debug]]
- [[Log.Error_2]]
- [[Log.Error]]

**Called-by <-**
- [[DspCommand.Create]]

