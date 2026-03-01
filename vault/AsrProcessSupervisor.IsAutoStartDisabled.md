---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
---
# AsrProcessSupervisor::IsAutoStartDisabled
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`


#### [[AsrProcessSupervisor.IsAutoStartDisabled]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsAutoStartDisabled()
```

**Called-by <-**
- [[AsrProcessSupervisor.EnsureServiceReadyAsync]]
- [[AsrProcessSupervisor.TriggerBackgroundWarmup]]

