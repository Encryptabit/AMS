---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# DspCommand::BuildFilterGraph
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Constructs a sequential FFmpeg DSP graph from an input audio buffer and a configured filter chain.**

`BuildFilterGraph` starts from `FfFilterGraph.FromBuffer(buffer)` and iterates the provided `FilterConfig` list in order, rebinding `graph` each time with `definition.Apply(graph, parameters)`. Each step resolves the filter via `GetFilterDefinition(filter.Name)` and converts JSON config through `DeserializeParameters`, so unknown filter names or invalid parameter payloads fail through those helpers while valid/empty payloads can fall back to defaults.


#### [[DspCommand.BuildFilterGraph]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FfFilterGraph BuildFilterGraph(AudioBuffer buffer, IReadOnlyList<FilterConfig> filters)
```

**Calls ->**
- [[DspCommand.DeserializeParameters]]
- [[DspCommand.GetFilterDefinition]]
- [[FfFilterGraph.FromBuffer]]

**Called-by <-**
- [[DspCommand.ExecuteFilterChain]]

