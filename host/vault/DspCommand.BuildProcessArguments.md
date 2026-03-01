---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 25
fan_in: 1
fan_out: 1
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
---
# DspCommand::BuildProcessArguments
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

> [!danger] High Complexity (25)
> Cyclomatic complexity: 25. Consider refactoring into smaller methods.

## Summary
**Construct a validated, precedence-aware command-line argument list for processing a single treatment node in the chain runner.**

`BuildProcessArguments` assembles the DSP process CLI argv by starting with `process`, resolving `node.Plugin` through `ResolvePath`, adding one `--input=` per input, optional `--midiInput=`, required `--output=`, and `--overwrite`. It computes numeric options with precedence `override* ?? node.* ?? chain.*`, and for bit depth additionally falls back to `DefaultBitDepth`, emitting `--sampleRate`, `--blockSize`, and `--outChannels` only when present while always emitting `--bitDepth`. It also resolves optional `ParameterFile` and `Preset` paths, appends non-empty `--param=` entries from `node.Parameters`, includes non-blank `AdditionalArguments` as-is, and returns the built `List<string>` as `IReadOnlyList<string>`.


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

