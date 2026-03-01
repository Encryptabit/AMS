---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 7
tags:
  - method
  - llm/async
  - llm/utility
  - llm/factory
  - llm/error-handling
---
# MfaProcessSupervisor::StartProcessAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Summary
**Start or restart the managed MFA PowerShell process and bring it to a ready state with background IO pumping.**

`StartProcessAsync` serializes supervisor startup inside `StartLock`: it no-ops if `_process` is alive and `_isReady`, otherwise it calls `TearDownProcess`, `EnsureBootstrapScript`, builds `ProcessStartInfo` from `ResolvePwshExecutable()`, appends PowerShell arguments, optionally sets `MFA_ROOT_DIR` from `ResolveWorkspaceRootForSupervisor`, and starts the child process with redirected stdio. It initializes runtime plumbing (`_stdin` auto-flush, `_pumpCts`, unbounded `_lineChannel`, `_stdoutPump`/`_stderrPump` via `PumpAsync`, `_activePumpCount = 2`) and resets `_isReady` before awaiting readiness. After `WaitForReadyAsync(CancellationToken.None)` it marks `_isReady = true`, and a `finally` block always clears `_startTask` under lock so subsequent startup attempts can proceed after success or failure.


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

