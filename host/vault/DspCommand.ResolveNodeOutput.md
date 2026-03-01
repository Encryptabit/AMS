---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# DspCommand::ResolveNodeOutput
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Compute the output audio file path for a chain node, using an explicit path when configured or a generated safe default filename when not.**

`ResolveNodeOutput` determines the filesystem output path for a DSP treatment node. If `node.OutputFile` is provided, it resolves that value against `workRoot` using `ResolvePath`, creates the containing directory via `Directory.CreateDirectory(Path.GetDirectoryName(path)!)`, and returns the resolved path. Otherwise it builds a default filename from a stem (`node.Name` or `Path.GetFileNameWithoutExtension(node.Plugin)`), sanitizes invalid filename characters with `Sanitize`, prefixes a 1-based zero-padded index (`{index + 1:00}`), appends `.wav`, and combines it with `workRoot`.


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

