---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# Log::IsDebugLoggingEnabled
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/Log.cs`


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

