---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 3
tags:
  - method
---
# AsrProcessSupervisor::BuildStartInfo
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`


#### [[AsrProcessSupervisor.BuildStartInfo]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ProcessStartInfo BuildStartInfo()
```

**Calls ->**
- [[AsrProcessSupervisor.CreateStartInfoForScript]]
- [[AsrProcessSupervisor.ResolvePowerShell]]
- [[AsrProcessSupervisor.ResolveRepoRoot]]

**Called-by <-**
- [[AsrProcessSupervisor.StartManagedProcess]]

