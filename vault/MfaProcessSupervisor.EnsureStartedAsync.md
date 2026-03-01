---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 5
fan_in: 3
fan_out: 1
tags:
  - method
---
# MfaProcessSupervisor::EnsureStartedAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`


#### [[MfaProcessSupervisor.EnsureStartedAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task EnsureStartedAsync(CancellationToken cancellationToken)
```

**Calls ->**
- [[MfaProcessSupervisor.StartProcessAsync]]

**Called-by <-**
- [[MfaProcessSupervisor.EnsureReadyAsync]]
- [[MfaProcessSupervisor.RunAsync]]
- [[MfaProcessSupervisor.TriggerBackgroundWarmup]]

