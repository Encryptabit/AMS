---
namespace: "Ams.Core.Common"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Common/Log.cs"
access_modifier: "public"
complexity: 1
fan_in: 9
fan_out: 0
tags:
  - method
---
# Log::Error
**Path**: `home/cari/repos/AMS/host/Ams.Core/Common/Log.cs`


#### [[Log.Error_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void Error(string message, params object[] args)
```

**Called-by <-**
- [[DspCommand.CreateInitCommand]]
- [[DspCommand.CreateSetDirAddCommand]]
- [[PipelineCommand.CreatePrepRenameCommand]]
- [[PipelineCommand.RunStats]]
- [[TreatCommand.Create]]
- [[ValidateCommand.CreateReportCommand]]
- [[ValidateCommand.CreateServeCommand]]
- [[ValidateCommand.CreateTimingCommand]]
- [[ValidateCommand.CreateTimingInitCommand]]

