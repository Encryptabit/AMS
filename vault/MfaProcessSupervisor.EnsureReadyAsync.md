---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "internal"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
---
# MfaProcessSupervisor::EnsureReadyAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`


#### [[MfaProcessSupervisor.EnsureReadyAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static Task EnsureReadyAsync(CancellationToken cancellationToken)
```

**Calls ->**
- [[MfaProcessSupervisor.EnsureStartedAsync]]

**Called-by <-**
- [[MfaWorkflow.RunChapterAsync]]
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]

