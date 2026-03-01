---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# PipelineCommand::LogStageInfo
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Conditionally write pipeline stage diagnostic messages to the debug logger when quiet mode is not enabled.**

`LogStageInfo` is a static helper in `PipelineCommand` that gates debug-stage logging behind a boolean flag. Its implementation is an early-return guard (`if (quiet) return;`) followed by a direct pass-through to `Log.Debug(message, args)`, so message templates and variadic arguments are forwarded unchanged.


#### [[PipelineCommand.LogStageInfo]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void LogStageInfo(bool quiet, string message, params object[] args)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[PipelineCommand.RunPipelineAsync]]

