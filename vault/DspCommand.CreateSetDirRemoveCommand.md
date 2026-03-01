---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 3
tags:
  - method
---
# DspCommand::CreateSetDirRemoveCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.CreateSetDirRemoveCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateSetDirRemoveCommand()
```

**Calls ->**
- [[DspConfigService.LoadAsync]]
- [[DspConfigService.SaveAsync]]
- [[Log.Debug]]

**Called-by <-**
- [[DspCommand.CreateSetDirCommand]]

