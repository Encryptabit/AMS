---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# DspCommand::SerializeParameters
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Serialize filter parameter input into a JSON element suitable for writing filter-chain configuration.**

SerializeParameters is an expression-bodied helper that calls JsonSerializer.SerializeToElement using FilterChainConfig.SerializerOptions to convert a filter parameter object into a JsonElement. Its null-coalescing fallback (value ?? new { }) guarantees callers such as CreateFilterChainInitCommand and CreateFilterConfig always store a non-null, object-shaped JSON payload in FilterConfig.Parameters.


#### [[DspCommand.SerializeParameters]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static JsonElement SerializeParameters(object value)
```

**Called-by <-**
- [[DspCommand.CreateFilterChainInitCommand]]
- [[DspCommand.CreateFilterConfig]]

