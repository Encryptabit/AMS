---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# DspCommand::ResolveInputs
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Resolve a treatment node’s concrete audio inputs and optional MIDI input from chain context and tokenized configuration.**

`ResolveInputs` builds the effective input set for a node by creating a `List<string>`, defaulting to `previousOutput` when `node.Inputs` is null/empty, or expanding each configured input token via `ExpandInputToken(baseDirectory, initialInput, previousOutput)`. It also conditionally resolves `node.MidiInput` only when it is non-null/non-whitespace, returning it as a nullable `string?`. The method is used by `RunChainAsync` to prepare per-node input arguments before process invocation.


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

