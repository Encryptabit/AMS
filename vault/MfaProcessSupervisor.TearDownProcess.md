---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 15
fan_in: 2
fan_out: 2
tags:
  - method
  - danger/high-complexity
---
# MfaProcessSupervisor::TearDownProcess
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

> [!danger] High Complexity (15)
> Cyclomatic complexity: 15. Consider refactoring into smaller methods.


#### [[MfaProcessSupervisor.TearDownProcess]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void TearDownProcess()
```

**Calls ->**
- [[MfaProcessSupervisor.IsProcessRunning]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaProcessSupervisor.Shutdown]]
- [[MfaProcessSupervisor.StartProcessAsync]]

