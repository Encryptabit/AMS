---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/factory
  - llm/di
  - llm/utility
---
# Log::ConfigureDefaults
**Path**: `Projects/AMS/host/Ams.Core/Common/Log.cs`

## Summary
**Initialize the shared AMS logging pipeline with default configuration and return the configured logger factory.**

`ConfigureDefaults` is a thin convenience wrapper that creates a preconfigured logging factory via `CreateDefaultFactory(...)` using the provided sink/retention/level parameters, then installs it as the process-wide shared logger through `Configure(factory)`. It returns the same `ILoggerFactory` instance so callers can both initialize global logging and retain a reference for DI or disposal. The method itself contains no branching beyond delegation.


#### [[Log.ConfigureDefaults]]
##### What it does:
<member name="M:Ams.Core.Common.Log.ConfigureDefaults(System.String,System.String,System.Int64,System.Int32,System.Boolean,System.Nullable{Serilog.Events.LogEventLevel})">
    <summary>
    Configures the shared logger using the default factory settings and returns the factory.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static ILoggerFactory ConfigureDefaults(string baseDirectory = null, string logFileName = "ams-log.txt", long fileSizeLimitBytes = 10485760, int retainedFileCountLimit = 5, bool includeConsole = true, LogEventLevel? minimumLevel = null)
```

**Calls ->**
- [[Log.Configure]]
- [[Log.CreateDefaultFactory]]

**Called-by <-**
- [[Program.Main]]

