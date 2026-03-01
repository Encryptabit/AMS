---
namespace: "Ams.Cli.Models"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Models/TreatmentModels.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TreatmentNode::.ctor
**Path**: `Projects/AMS/host/Ams.Cli/Models/TreatmentModels.cs`

## Summary
**Initialize a treatment-node model that encapsulates all per-step plugin execution and audio configuration for CLI pipeline processing.**

This constructor builds a `TreatmentNode` configuration object from full treatment/plugin metadata (`name`, `plugin`, `description`, `preset`, parameter sources), audio overrides (`sampleRate`, `blockSize`, `outChannels`, `bitDepth`), routing (`inputs`, `midiInput`), and passthrough CLI args (`parameters`, `additionalArguments`, `outputFile`). The `IReadOnlyList<string>` inputs and nullable integer fields indicate an immutable-facing model with optional per-node runtime overrides. With cyclomatic complexity 2, the implementation is effectively direct property initialization with only minimal branching (likely simple normalization/guard logic).


#### [[TreatmentNode..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public TreatmentNode(string name, string plugin, string description, IReadOnlyList<string> parameters, string parameterFile, string preset, int? sampleRate, int? blockSize, int? outChannels, int? bitDepth, IReadOnlyList<string> inputs, string midiInput, IReadOnlyList<string> additionalArguments, string outputFile)
```

