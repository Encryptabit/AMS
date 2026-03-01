---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 5
tags:
  - method
---
# DspCommand::CreateChainListCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.CreateChainListCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateChainListCommand()
```

**Calls ->**
- [[DspCommand.LoadChainAsync]]
- [[DspCommand.ResolveChainFile]]
- [[DspCommand.ResolvePath]]
- [[DspCommand.TryGetFriendlyName]]
- [[DspConfigService.LoadAsync]]

**Called-by <-**
- [[DspCommand.CreateChainCommand]]

