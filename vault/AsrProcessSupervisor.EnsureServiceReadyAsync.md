---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "internal"
complexity: 8
fan_in: 2
fan_out: 7
tags:
  - method
---
# AsrProcessSupervisor::EnsureServiceReadyAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`


#### [[AsrProcessSupervisor.EnsureServiceReadyAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static Task EnsureServiceReadyAsync(string baseUrl, CancellationToken cancellationToken)
```

**Calls ->**
- [[AsrProcessSupervisor.IsAutoStartDisabled]]
- [[AsrProcessSupervisor.IsHealthyAsync]]
- [[AsrProcessSupervisor.IsLocalBaseUrl]]
- [[AsrProcessSupervisor.KillProcess]]
- [[AsrProcessSupervisor.StartManagedProcess]]
- [[AsrProcessSupervisor.WaitForHealthyAsync]]
- [[Log.Debug]]

**Called-by <-**
- [[GenerateTranscriptCommand.RunNemoAsync]]
- [[AsrProcessSupervisor.TriggerBackgroundWarmup]]

