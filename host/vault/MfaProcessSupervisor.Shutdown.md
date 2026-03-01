---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "internal"
complexity: 3
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# MfaProcessSupervisor::Shutdown
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Summary
**Gracefully signal the MFA supervisor to exit and then forcefully tear down all managed process resources.**

`Shutdown` serializes teardown with `lock (StartLock)` to avoid races with startup/command paths. If `_stdin` is available, it attempts graceful termination by logging, writing `QuitToken`, and flushing; signaling failures are caught and logged without aborting cleanup. It then always calls `TearDownProcess()` to perform hard cleanup of process and pump resources.


#### [[MfaProcessSupervisor.Shutdown]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static void Shutdown()
```

**Calls ->**
- [[MfaProcessSupervisor.TearDownProcess]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaProcessSupervisor.RegisterForShutdown]]
- [[PipelineService.RunChapterAsync]]

