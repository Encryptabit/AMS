---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "internal"
complexity: 7
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/error-handling
  - llm/validation
---
# AsrProcessSupervisor::Shutdown
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

## Summary
**It safely stops and cleans up the supervised Nemo ASR process during application shutdown.**

`Shutdown` performs synchronized teardown of the managed ASR process under `ShutdownLock`, with a no-op fast return when Nemo mode is disabled or no process is tracked. If the supervisor owns a still-running process, it attempts graceful termination via `_process.Kill(true)` and waits up to 10 seconds for exit, logging timeouts/errors at debug level. In `finally`, it always disposes the process handle, clears `_process`/`_ownsProcess`, and resets supervisor state to `Idle`.


#### [[AsrProcessSupervisor.Shutdown]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static void Shutdown()
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[AsrProcessSupervisor.RegisterForShutdown]]

