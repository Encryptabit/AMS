---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 8
fan_out: 0
tags:
  - method
---
# DspCommand::ResolvePath
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.ResolvePath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolvePath(string path, string baseDirectory)
```

**Called-by <-**
- [[DspCommand.BuildProcessArguments]]
- [[DspCommand.BuildSingleNodeChain]]
- [[DspCommand.CreateChainListCommand]]
- [[DspCommand.CreateListParamsCommand]]
- [[DspCommand.CreateNodeFromOptions]]
- [[DspCommand.ExpandInputToken]]
- [[DspCommand.ResolveNodeOutput]]
- [[DspCommand.ResolvePluginPath]]

