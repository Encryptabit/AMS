---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 3
fan_out: 0
tags:
  - method
---
# DspCommand::TryGetFriendlyName
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.TryGetFriendlyName]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string TryGetFriendlyName(DspConfig config, string pluginPath)
```

**Called-by <-**
- [[DspCommand.BuildSingleNodeChain]]
- [[DspCommand.CreateChainListCommand]]
- [[DspCommand.CreateNodeFromOptions]]

