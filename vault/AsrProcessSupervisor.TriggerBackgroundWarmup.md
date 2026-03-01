---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "internal"
complexity: 4
fan_in: 1
fan_out: 3
tags:
  - method
---
# AsrProcessSupervisor::TriggerBackgroundWarmup
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`


#### [[AsrProcessSupervisor.TriggerBackgroundWarmup]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static void TriggerBackgroundWarmup(string baseUrl)
```

**Calls ->**
- [[AsrProcessSupervisor.EnsureServiceReadyAsync]]
- [[AsrProcessSupervisor.IsAutoStartDisabled]]
- [[Log.Debug]]

**Called-by <-**
- [[Program.Main]]

