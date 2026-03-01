---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/di
  - llm/validation
  - llm/error-handling
---
# Log::Configure
**Path**: `Projects/AMS/host/Ams.Core/Common/Log.cs`

## Summary
**Configures the static `Log` facade to use a provided logger factory and category-specific logger instance.**

`Configure` performs a null guard on the injected `ILoggerFactory` and throws `ArgumentNullException` if missing. It uses `lock (SyncRoot)` to atomically update the class-level `loggerFactory` and rebuild the shared `logger` instance. The category passed to `CreateLogger` falls back to `DefaultCategory` when `category` is null, empty, or whitespace.


#### [[Log.Configure]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void Configure(ILoggerFactory factory, string category = null)
```

**Called-by <-**
- [[Log.ConfigureDefaults]]

