---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::TryReadTiming
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Attempt to read a sentence’s start/end timing from flexible JSON field layouts and indicate success only when a complete timing pair is available.**

`TryReadTiming` normalizes timing extraction from a `JsonElement` sentence by probing multiple schema variants in priority order. It initializes `start`/`end` to `double.NaN`, then attempts paired reads via `TryGetDouble` from `sentence.timing` (`startSec`/`endSec`, then `start`/`end`) before falling back to top-level fields with the same two key pairs. The method returns `true` only when both values in a pair are present and parsed, and on total failure it explicitly restores both outputs to `NaN` before returning `false`.


#### [[PipelineCommand.TryReadTiming]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryReadTiming(JsonElement sentence, out double start, out double end)
```

**Calls ->**
- [[PipelineCommand.TryGetDouble]]

**Called-by <-**
- [[PipelineCommand.LoadSentenceTimings]]

