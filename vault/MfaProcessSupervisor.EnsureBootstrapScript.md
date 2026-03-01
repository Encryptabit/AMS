---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
---
# MfaProcessSupervisor::EnsureBootstrapScript
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`


#### [[MfaProcessSupervisor.EnsureBootstrapScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void EnsureBootstrapScript()
```

**Calls ->**
- [[MfaProcessSupervisor.BuildSupervisorScript]]
- [[MfaProcessSupervisor.ResolveBootstrapSequence]]

**Called-by <-**
- [[MfaProcessSupervisor.StartProcessAsync]]

