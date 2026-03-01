---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 2
tags:
  - method
---
# AsrProcessSupervisor::CreateStartInfoForScript
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`


#### [[AsrProcessSupervisor.CreateStartInfoForScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ProcessStartInfo CreateStartInfoForScript(string scriptPath)
```

**Calls ->**
- [[AsrProcessSupervisor.ResolvePowerShell]]
- [[Log.Debug]]

**Called-by <-**
- [[AsrProcessSupervisor.BuildStartInfo]]

