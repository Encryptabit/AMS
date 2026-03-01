---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 1
tags:
  - method
---
# AsrProcessSupervisor::ResolvePowerShell
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`


#### [[AsrProcessSupervisor.ResolvePowerShell]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolvePowerShell()
```

**Calls ->**
- [[AsrProcessSupervisor.TryFindOnPath]]

**Called-by <-**
- [[AsrProcessSupervisor.BuildStartInfo]]
- [[AsrProcessSupervisor.CreateStartInfoForScript]]

