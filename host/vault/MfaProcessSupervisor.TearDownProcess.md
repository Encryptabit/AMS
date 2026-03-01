---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 15
fan_in: 2
fan_out: 2
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/error-handling
  - llm/data-access
  - llm/validation
---
# MfaProcessSupervisor::TearDownProcess
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

> [!danger] High Complexity (15)
> Cyclomatic complexity: 15. Consider refactoring into smaller methods.

## Summary
**Fully tear down MFA supervisor process resources, IO plumbing, and temporary bootstrap artifacts while tolerating shutdown-time failures.**

`TearDownProcess` aggressively resets supervisor runtime state by canceling/disposing `_pumpCts`, clearing pump task references/counters, completing and nulling `_lineChannel`, and resetting `_startTask`/`_isReady`. It then terminates the child process defensively: `IsProcessRunning(_process)` gates `Kill(true)` and `WaitForExit(2000)`, with `InvalidOperationException` and other termination issues handled without propagating, while wait failures are debug-logged. Finally it logs termination, disposes and nulls `_process`, clears `_stdin`, and deletes the generated bootstrap script at `_scriptPath` when present (logging deletion failures) before nulling the path.


#### [[MfaProcessSupervisor.TearDownProcess]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void TearDownProcess()
```

**Calls ->**
- [[MfaProcessSupervisor.IsProcessRunning]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaProcessSupervisor.Shutdown]]
- [[MfaProcessSupervisor.StartProcessAsync]]

