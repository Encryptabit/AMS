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
# DspCommand::CreateTestAllCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.CreateTestAllCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateTestAllCommand()
```

**Calls ->**
- [[DspCommand.CreateFilterConfig]]
- [[DspCommand.ExecuteFilterChain]]
- [[CommandInputResolver.RequireAudio]]
- [[Log.Error]]

**Called-by <-**
- [[DspCommand.Create]]

