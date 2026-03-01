---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# DspCommand::DeserializeParameters
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Deserializes a filter’s JSON parameters into the expected parameter type, with fallback to configured/default instances and wrapped deserialization errors.**

`DeserializeParameters` first returns `null` when `definition.ParameterType` is `null` (parameterless filters). It treats `Undefined`, `Null`, and empty-object JSON as "use defaults", returning `definition.DefaultParameters` or `CreateDefaultParameterInstance(definition)`. For non-empty input, it calls `element.Deserialize(definition.ParameterType, FilterChainConfig.SerializerOptions)` and still null-coalesces to the same default chain. It catches `JsonException` and rethrows `InvalidOperationException` with the filter name for contextual failure reporting during `BuildFilterGraph`.


#### [[DspCommand.DeserializeParameters]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static object DeserializeParameters(DspCommand.FilterDefinition definition, JsonElement element)
```

**Calls ->**
- [[DspCommand.CreateDefaultParameterInstance]]

**Called-by <-**
- [[DspCommand.BuildFilterGraph]]

