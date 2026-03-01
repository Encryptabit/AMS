---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 2
tags:
  - method
---
# MfaProcessSupervisor::ResolveWorkspaceRootForSupervisor
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`


#### [[MfaProcessSupervisor.ResolveWorkspaceRootForSupervisor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveWorkspaceRootForSupervisor()
```

**Calls ->**
- [[MfaWorkspaceResolver.ResolvePreferredRoot]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaProcessSupervisor.StartProcessAsync]]

