---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "public"
complexity: 2
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
**Creates a parameterized DSP filter definition by wrapping a typed filter-applier and default parameter object into the command’s generic runtime filter model.**

`Create<TParams>` is a typed factory for the polymorphic `FilterDefinition` record, adapting `Func<FfFilterGraph, TParams, FfFilterGraph>` into the record’s `Func<FfFilterGraph, object?, FfFilterGraph>` shape. It eagerly computes `defaultParams` from `defaults` or `Activator.CreateInstance(typeof(TParams))!`, stores `typeof(TParams)` as `ParameterType`, and the wrapper lambda casts `value ?? defaultParams` back to `TParams` before invoking `apply`. This keeps authoring strongly typed while persisting a uniform object-based execution contract with deterministic fallback parameters.


#### [[FilterDefinition.Create_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static DspCommand.FilterDefinition Create<TParams>(string name, Func<FfFilterGraph, TParams, FfFilterGraph> apply, TParams defaults = null) where TParams : class
```

