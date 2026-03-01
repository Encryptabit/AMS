---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "internal"
complexity: 3
fan_in: 2
fan_out: 0
tags:
  - method
---
# MfaProcessSupervisor::ResolvePwshExecutable
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`


#### [[MfaProcessSupervisor.ResolvePwshExecutable]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static string ResolvePwshExecutable()
```

**Called-by <-**
- [[MfaDetachedProcessRunner.RunAsync]]
- [[MfaProcessSupervisor.StartProcessAsync]]

