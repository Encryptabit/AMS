---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 1
fan_in: 9
fan_out: 0
tags:
  - method
  - llm/utility
---
# Log::Error
**Path**: `Projects/AMS/host/Ams.Core/Common/Log.cs`

## Summary
**Emits an error-level log message via the centralized `Log` facade using a message template and arguments.**

`Error(string message, params object[] args)` is an expression-bodied adapter that directly calls `logger.LogError(message, args)` on the shared static logger. It does not add control flow, exception metadata, or input validation, so formatting and structured argument binding are handled by the underlying logging abstractions.


#### [[Log.Error_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void Error(string message, params object[] args)
```

**Called-by <-**
- [[DspCommand.CreateInitCommand]]
- [[DspCommand.CreateSetDirAddCommand]]
- [[PipelineCommand.CreatePrepRenameCommand]]
- [[PipelineCommand.RunStats]]
- [[TreatCommand.Create]]
- [[ValidateCommand.CreateReportCommand]]
- [[ValidateCommand.CreateServeCommand]]
- [[ValidateCommand.CreateTimingCommand]]
- [[ValidateCommand.CreateTimingInitCommand]]

