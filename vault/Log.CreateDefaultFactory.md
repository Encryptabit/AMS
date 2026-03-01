---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
---
# Log::CreateDefaultFactory
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/Log.cs`


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

