---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 3
tags:
  - method
---
# DspCommand::BuildSingleNodeChain
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.BuildSingleNodeChain]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static TreatmentChain BuildSingleNodeChain(string plugin, IReadOnlyList<string> parameters, string preset, FileInfo paramFile, string baseDirectory, DspConfig config = null)
```

**Calls ->**
- [[DspCommand.ResolvePath]]
- [[DspCommand.ResolvePluginPath]]
- [[DspCommand.TryGetFriendlyName]]

**Called-by <-**
- [[DspCommand.CreateRunCommand]]

