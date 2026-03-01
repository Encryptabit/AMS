---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
  - llm/data-access
---
# PipelineCommand::TryGetInt
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Safely extract an integer from a JSON property (number or numeric string) using a non-throwing try-pattern.**

`TryGetInt` initializes `value` to `0`, then uses `element.TryGetProperty(propertyName, out var prop)` to guard against missing fields and return `false` early. It supports two parse paths: JSON numeric tokens (`prop.ValueKind == JsonValueKind.Number` + `prop.TryGetInt32(out value)`) and numeric strings (`prop.ValueKind == JsonValueKind.String` + `int.TryParse(prop.GetString(), out value)`). If neither path succeeds, it returns `false`, which lets `LoadSentenceTimings` skip malformed sentence entries instead of failing.


#### [[PipelineCommand.TryGetInt]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryGetInt(JsonElement element, string propertyName, out int value)
```

**Called-by <-**
- [[PipelineCommand.LoadSentenceTimings]]

