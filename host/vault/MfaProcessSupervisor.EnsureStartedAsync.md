---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 5
fan_in: 3
fan_out: 1
tags:
  - method
  - llm/async
  - llm/utility
  - llm/error-handling
---
# MfaProcessSupervisor::EnsureStartedAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Summary
**Ensure the MFA supervisor process is started exactly once and expose an awaitable readiness gate with per-caller cancellation.**

`EnsureStartedAsync` coordinates singleton startup using `StartLock` and a shared `_startTask` so concurrent callers coalesce onto one initialization task. Inside the lock it fast-returns `Task.CompletedTask` when `_process` is alive and `_isReady` is true; otherwise it creates `_startTask = StartProcessAsync()` only if absent/completed and captures it locally. Outside the lock it returns `startTask.WaitAsync(cancellationToken)`, so callers observe startup completion while applying their own cancellation without canceling the shared startup task itself.


#### [[MfaProcessSupervisor.EnsureStartedAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task EnsureStartedAsync(CancellationToken cancellationToken)
```

**Calls ->**
- [[MfaProcessSupervisor.StartProcessAsync]]

**Called-by <-**
- [[MfaProcessSupervisor.EnsureReadyAsync]]
- [[MfaProcessSupervisor.RunAsync]]
- [[MfaProcessSupervisor.TriggerBackgroundWarmup]]

