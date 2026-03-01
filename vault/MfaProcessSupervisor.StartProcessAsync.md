---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 7
tags:
  - method
---
# MfaProcessSupervisor::StartProcessAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`


#### [[MfaProcessSupervisor.StartProcessAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task StartProcessAsync()
```

**Calls ->**
- [[MfaProcessSupervisor.EnsureBootstrapScript]]
- [[MfaProcessSupervisor.PumpAsync]]
- [[MfaProcessSupervisor.ResolvePwshExecutable]]
- [[MfaProcessSupervisor.ResolveWorkspaceRootForSupervisor]]
- [[MfaProcessSupervisor.TearDownProcess]]
- [[MfaProcessSupervisor.WaitForReadyAsync]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaProcessSupervisor.EnsureStartedAsync]]

