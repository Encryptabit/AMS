---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
---
# DspCommand::ResolveNodeOutput
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.ResolveNodeOutput]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveNodeOutput(TreatmentNode node, string workRoot, int index)
```

**Calls ->**
- [[DspCommand.ResolvePath]]
- [[DspCommand.Sanitize]]

**Called-by <-**
- [[DspCommand.RunChainAsync]]

