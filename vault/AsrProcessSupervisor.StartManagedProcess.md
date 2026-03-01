---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 2
tags:
  - method
---
# AsrProcessSupervisor::StartManagedProcess
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`


#### [[AsrProcessSupervisor.StartManagedProcess]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void StartManagedProcess()
```

**Calls ->**
- [[AsrProcessSupervisor.BuildStartInfo]]
- [[Log.Debug]]

**Called-by <-**
- [[AsrProcessSupervisor.EnsureServiceReadyAsync]]

