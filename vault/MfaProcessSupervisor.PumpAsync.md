---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 1
tags:
  - method
---
# MfaProcessSupervisor::PumpAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`


#### [[MfaProcessSupervisor.PumpAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task PumpAsync(StreamReader reader, MfaProcessSupervisor.StreamKind kind, CancellationToken cancellationToken)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[MfaProcessSupervisor.StartProcessAsync]]

