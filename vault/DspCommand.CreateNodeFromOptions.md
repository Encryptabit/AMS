---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 14
fan_in: 3
fan_out: 3
tags:
  - method
---
# DspCommand::CreateNodeFromOptions
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.CreateNodeFromOptions]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static TreatmentNode CreateNodeFromOptions(InvocationContext context, DspCommand.NodeOptionBundle options, string baseDirectory, DspConfig config)
```

**Calls ->**
- [[DspCommand.ResolvePath]]
- [[DspCommand.ResolvePluginPath]]
- [[DspCommand.TryGetFriendlyName]]

**Called-by <-**
- [[DspCommand.CreateChainAddCommand]]
- [[DspCommand.CreateChainInsertCommand]]
- [[DspCommand.CreateChainPrependCommand]]

