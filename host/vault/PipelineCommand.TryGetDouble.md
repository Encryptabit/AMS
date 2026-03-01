---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
  - llm/data-access
---
# PipelineCommand::TryGetDouble
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Safely read a JSON property as a `double`, tolerating both numeric and string-encoded values.**

`TryGetDouble` is a helper used by `TryReadTiming` that initializes `value` to `double.NaN`, then attempts `element.TryGetProperty(propertyName, out var prop)`. It returns `true` when the property is either a `JsonValueKind.Number` (`prop.GetDouble()`) or a `JsonValueKind.String` parseable via `double.TryParse`; otherwise it returns `false` and leaves the sentinel `NaN` value.


#### [[PipelineCommand.TryGetDouble]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryGetDouble(JsonElement element, string propertyName, out double value)
```

**Called-by <-**
- [[PipelineCommand.TryReadTiming]]

