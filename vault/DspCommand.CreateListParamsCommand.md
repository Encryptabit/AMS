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
# DspCommand::CreateListParamsCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.CreateListParamsCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateListParamsCommand()
```

**Calls ->**
- [[DspCommand.ResolvePath]]
- [[PlugalyzerService.RunAsync]]
- [[Log.Error]]

**Called-by <-**
- [[DspCommand.Create]]

