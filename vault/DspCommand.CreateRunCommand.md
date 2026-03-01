---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 9
tags:
  - method
---
# DspCommand::CreateRunCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.CreateRunCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateRunCommand()
```

**Calls ->**
- [[DspCommand.BuildSingleNodeChain]]
- [[DspCommand.LoadChainAsync]]
- [[DspCommand.ResolveChainFile]]
- [[DspCommand.ResolveOutputFile]]
- [[DspCommand.RunChainAsync]]
- [[DspConfigService.LoadAsync]]
- [[CommandInputResolver.RequireAudio]]
- [[Log.Debug]]
- [[Log.Error]]

**Called-by <-**
- [[DspCommand.Create]]

