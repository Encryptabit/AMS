---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# Log::IsDebugLoggingEnabled
**Path**: `Projects/AMS/host/Ams.Core/Common/Log.cs`

## Summary
**Determine whether current environment-based logging configuration allows debug-level output.**

`IsDebugLoggingEnabled()` resolves the effective minimum log level from environment settings by calling `ResolveMinimumLevelFromEnvironment()`. It returns `true` when the resolved level is `<= LogEventLevel.Debug`, meaning debug (and more verbose) events are enabled. The method is side-effect free and serves as a lightweight runtime gate for debug-only behavior.


#### [[Log.IsDebugLoggingEnabled]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static bool IsDebugLoggingEnabled()
```

**Calls ->**
- [[Log.ResolveMinimumLevelFromEnvironment]]

**Called-by <-**
- [[PipelineCommand.CreateRun]]

