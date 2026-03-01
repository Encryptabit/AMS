---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 1
fan_in: 6
fan_out: 0
tags:
  - method
---
# Log::Info
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/Log.cs`


#### [[Log.Info]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void Info(string message, params object[] args)
```

**Called-by <-**
- [[DspCommand.CreateFilterChainInitCommand]]
- [[DspCommand.CreateFilterChainRunCommand]]
- [[DspCommand.CreateFiltersCommand]]
- [[DspCommand.ExecuteFilterChain]]
- [[TreatCommand.Create]]
- [[AsrEngineConfig.DownloadModelIfMissingAsync]]

