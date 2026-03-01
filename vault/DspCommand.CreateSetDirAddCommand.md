---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 4
tags:
  - method
---
# DspCommand::CreateSetDirAddCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.CreateSetDirAddCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateSetDirAddCommand()
```

**Calls ->**
- [[DspConfigService.LoadAsync]]
- [[DspConfigService.SaveAsync]]
- [[Log.Debug]]
- [[Log.Error_2]]

**Called-by <-**
- [[DspCommand.CreateSetDirCommand]]

