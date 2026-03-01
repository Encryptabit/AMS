---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/utility
---
# FilterDefinition::Create
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Creates a `FilterDefinition` for DSP filters that operate without external parameter objects.**

This static factory overload in `DspCommand.FilterDefinition` adapts a parameterless filter delegate into the record’s unified `(FfFilterGraph, object?)` apply shape. It returns `new(name, null, (graph, _) => apply(graph), null)`, explicitly setting `ParameterType` and `DefaultParameters` to `null` while ignoring the runtime parameter argument in the wrapper lambda.


#### [[FilterDefinition.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static DspCommand.FilterDefinition Create(string name, Func<FfFilterGraph, FfFilterGraph> apply)
```

