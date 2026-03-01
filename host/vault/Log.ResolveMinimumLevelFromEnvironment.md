---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "private"
complexity: 8
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# Log::ResolveMinimumLevelFromEnvironment
**Path**: `Projects/AMS/host/Ams.Core/Common/Log.cs`

## Summary
**Converts an environment-provided log-level string into a `LogEventLevel` with deterministic defaulting behavior.**

`ResolveMinimumLevelFromEnvironment` reads the `AMS_LOG_LEVEL` environment variable (via `LevelEnvVar`) and immediately returns `DefaultMinimumLevel` when the value is null/empty/whitespace. It normalizes non-empty input with `Trim().ToUpperInvariant()` and uses a switch expression to map aliases (`TRACE`/`VERBOSE`, `INFO`/`INFORMATION`, `WARN`/`WARNING`, `FATAL`/`CRITICAL`) to `LogEventLevel` values. Unrecognized values also fall back to `DefaultMinimumLevel`, preventing invalid configuration from breaking logger setup.


#### [[Log.ResolveMinimumLevelFromEnvironment]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static LogEventLevel ResolveMinimumLevelFromEnvironment()
```

**Called-by <-**
- [[Log.CreateDefaultFactory]]
- [[Log.IsDebugLoggingEnabled]]

