---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "internal"
complexity: 1
fan_in: 1
fan_out: 2
tags:
  - method
---
# MfaProcessSupervisor::TriggerBackgroundWarmup
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`


#### [[MfaProcessSupervisor.TriggerBackgroundWarmup]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static void TriggerBackgroundWarmup()
```

**Calls ->**
- [[MfaProcessSupervisor.EnsureStartedAsync]]
- [[Log.Debug]]

**Called-by <-**
- [[Program.Main]]

