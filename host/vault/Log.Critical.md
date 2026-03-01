---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# Log::Critical
**Path**: `Projects/AMS/host/Ams.Core/Common/Log.cs`

## Summary
**Emits a critical-severity log entry with exception context and formatted message arguments via the central `Log` facade.**

`Critical(Exception exception, string message, params object[] args)` is an expression-bodied passthrough to the shared static logger, calling `logger.LogCritical(exception, message, args)`. It performs no validation or branching, so exception capture, template rendering, and sink behavior are fully delegated to the configured `Microsoft.Extensions.Logging` provider pipeline.


#### [[Log.Critical]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void Critical(Exception exception, string message, params object[] args)
```

