---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "internal"
complexity: 3
fan_in: 2
fan_out: 2
tags:
  - method
---
# MfaProcessSupervisor::Shutdown
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`


#### [[MfaProcessSupervisor.Shutdown]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static void Shutdown()
```

**Calls ->**
- [[MfaProcessSupervisor.TearDownProcess]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaProcessSupervisor.RegisterForShutdown]]
- [[PipelineService.RunChapterAsync]]

