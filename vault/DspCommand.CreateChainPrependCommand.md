---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 6
tags:
  - method
---
# DspCommand::CreateChainPrependCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.CreateChainPrependCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateChainPrependCommand()
```

**Calls ->**
- [[DspCommand.CreateNodeFromOptions]]
- [[DspCommand.LoadChainAsync]]
- [[DspCommand.ResolveChainFile]]
- [[DspCommand.SaveChainAsync]]
- [[DspConfigService.LoadAsync]]
- [[Log.Debug]]

**Called-by <-**
- [[DspCommand.CreateChainCommand]]

