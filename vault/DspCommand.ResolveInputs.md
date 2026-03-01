---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
---
# DspCommand::ResolveInputs
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.ResolveInputs]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (IReadOnlyList<string> Inputs, string MidiInput) ResolveInputs(TreatmentNode node, string baseDirectory, string initialInput, string previousOutput)
```

**Calls ->**
- [[DspCommand.ExpandInputToken]]

**Called-by <-**
- [[DspCommand.RunChainAsync]]

