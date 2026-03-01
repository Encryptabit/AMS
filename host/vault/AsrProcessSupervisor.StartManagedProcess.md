---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrProcessSupervisor::StartManagedProcess
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

## Summary
**It launches and tracks the managed asr-nemo process, including log wiring and supervisor state updates.**

`StartManagedProcess` builds launch metadata via `BuildStartInfo()` and fails fast by setting `_state = Faulted` and throwing when no startup script/process info can be resolved. It logs the launch command, starts a `Process` with redirected stdout/stderr, wires output/error handlers to `Log.Debug`, and registers an `Exited` handler that marks supervisor state as faulted when an owned process dies. On successful start it begins async stream reads and records ownership (`_process`, `_ownsProcess = true`, `_state = Starting`); any startup exception is wrapped in an `InvalidOperationException` after forcing `Faulted` state.


#### [[AsrProcessSupervisor.StartManagedProcess]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void StartManagedProcess()
```

**Calls ->**
- [[AsrProcessSupervisor.BuildStartInfo]]
- [[Log.Debug]]

**Called-by <-**
- [[AsrProcessSupervisor.EnsureServiceReadyAsync]]

