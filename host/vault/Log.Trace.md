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
---
# Log::Trace
**Path**: `Projects/AMS/host/Ams.Core/Common/Log.cs`

## Summary
**Writes a trace-level log entry through the shared `Log` facade using a message template and arguments.**

`Trace` is an expression-bodied forwarding method that calls `logger.LogTrace(message, args)` on the class-level shared logger. It contains no branching, formatting logic, or argument validation, relying entirely on `Microsoft.Extensions.Logging` for message template processing and parameter handling.


#### [[Log.Trace]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void Trace(string message, params object[] args)
```

