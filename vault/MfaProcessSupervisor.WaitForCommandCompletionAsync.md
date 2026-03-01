---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
---
# MfaProcessSupervisor::WaitForCommandCompletionAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`


#### [[MfaProcessSupervisor.WaitForCommandCompletionAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<int> WaitForCommandCompletionAsync(List<string> stdout, List<string> stderr, CancellationToken cancellationToken)
```

**Called-by <-**
- [[MfaProcessSupervisor.RunAsync]]

