---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/factory
  - llm/di
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# Log::CreateDefaultFactory
**Path**: `Projects/AMS/host/Ams.Core/Common/Log.cs`

## Summary
**Create and return a preconfigured shared logging factory for AMS components with consistent Serilog sinks and levels.**

`CreateDefaultFactory` builds a Serilog-backed `Microsoft.Extensions.Logging.ILoggerFactory` with optional console output and size-rolled file output. It resolves `baseDirectory` from the argument or `LocalApplicationData` (falling back to `AppContext.BaseDirectory`), sets `LogDirectory`/`LogFilePath`, and ensures the log directory exists via `Directory.CreateDirectory`. It selects the minimum level from the explicit argument or `ResolveMinimumLevelFromEnvironment()`, configures sink templates and retention/size limits, then returns a factory created with `ClearProviders()` and `AddSerilog(serilogLogger, dispose: true)`.


#### [[Log.CreateDefaultFactory]]
##### What it does:
<member name="M:Ams.Core.Common.Log.CreateDefaultFactory(System.String,System.String,System.Int64,System.Int32,System.Boolean,System.Nullable{Serilog.Events.LogEventLevel})">
    <summary>
    Creates a preconfigured logger factory that writes to console and rolling text files.
    Consumers can share this to keep log formatting uniform.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static ILoggerFactory CreateDefaultFactory(string baseDirectory = null, string logFileName = "ams-log.txt", long fileSizeLimitBytes = 10485760, int retainedFileCountLimit = 5, bool includeConsole = true, LogEventLevel? minimumLevel = null)
```

**Calls ->**
- [[Log.ResolveMinimumLevelFromEnvironment]]

**Called-by <-**
- [[Log.ConfigureDefaults]]

