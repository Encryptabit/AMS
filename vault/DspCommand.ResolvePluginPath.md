---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 6
fan_in: 2
fan_out: 1
tags:
  - method
---
# DspCommand::ResolvePluginPath
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.ResolvePluginPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolvePluginPath(string token, string baseDirectory, DspConfig config)
```

**Calls ->**
- [[DspCommand.ResolvePath]]

**Called-by <-**
- [[DspCommand.BuildSingleNodeChain]]
- [[DspCommand.CreateNodeFromOptions]]

