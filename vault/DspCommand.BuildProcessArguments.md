---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 25
fan_in: 1
fan_out: 1
tags:
  - method
  - danger/high-complexity
---
# DspCommand::BuildProcessArguments
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`

> [!danger] High Complexity (25)
> Cyclomatic complexity: 25. Consider refactoring into smaller methods.


#### [[DspCommand.BuildProcessArguments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<string> BuildProcessArguments(TreatmentNode node, TreatmentChain chain, IReadOnlyList<string> inputs, string midiInput, string nodeOutput, string baseDirectory, int? overrideSampleRate, int? overrideBlockSize, int? overrideOutChannels, int? overrideBitDepth)
```

**Calls ->**
- [[DspCommand.ResolvePath]]

**Called-by <-**
- [[DspCommand.RunChainAsync]]

