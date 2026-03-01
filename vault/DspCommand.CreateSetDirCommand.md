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
# DspCommand::CreateSetDirCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.CreateSetDirCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateSetDirCommand()
```

**Calls ->**
- [[DspCommand.CreateSetDirAddCommand]]
- [[DspCommand.CreateSetDirClearCommand]]
- [[DspCommand.CreateSetDirListCommand]]
- [[DspCommand.CreateSetDirRemoveCommand]]

**Called-by <-**
- [[DspCommand.Create]]

