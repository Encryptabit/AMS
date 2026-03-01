---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 15
fan_in: 1
fan_out: 7
tags:
  - method
  - danger/high-complexity
  - llm/async
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# DspCommand::RunChainAsync
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

> [!danger] High Complexity (15)
> Cyclomatic complexity: 15. Consider refactoring into smaller methods.

## Summary
**Runs a DSP treatment chain node-by-node against an input file and writes the resulting processed audio to the requested output path.**

`RunChainAsync` validates that the input exists and output overwrite rules are satisfied, initializes a temp/work root, then processes `chain.Nodes` sequentially. For each node it resolves effective audio/MIDI inputs (`ResolveInputs`), computes node output (`ResolveNodeOutput`), builds plugalyzer arguments (`BuildProcessArguments`), logs progress, and awaits `RunAsync`, throwing on non-zero exit codes. It tracks intermediate outputs, copies the final produced file to `outputPath` if needed, and in `finally` performs temp cleanup (`TryDeleteDirectory` or `TryDeleteFile`) based on `keepTemp` and whether a custom work directory was provided, while checking cancellation each iteration.


#### [[DspCommand.RunChainAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task RunChainAsync(TreatmentChain chain, string inputPath, string outputPath, string chainBaseDirectory, int? overrideSampleRate, int? overrideBlockSize, int? overrideOutChannels, int? overrideBitDepth, DirectoryInfo workDirOption, bool keepTemp, bool overwrite, CancellationToken cancellationToken)
```

**Calls ->**
- [[DspCommand.BuildProcessArguments]]
- [[DspCommand.ResolveInputs]]
- [[DspCommand.ResolveNodeOutput]]
- [[DspCommand.TryDeleteDirectory]]
- [[DspCommand.TryDeleteFile]]
- [[PlugalyzerService.RunAsync]]
- [[Log.Debug]]

**Called-by <-**
- [[DspCommand.CreateRunCommand]]

