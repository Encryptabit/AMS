---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 2
tags:
  - method
---
# Log::ConfigureDefaults
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/Log.cs`


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

