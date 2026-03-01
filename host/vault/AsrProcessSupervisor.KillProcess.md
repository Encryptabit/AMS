---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/error-handling
  - llm/validation
---
# AsrProcessSupervisor::KillProcess
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

## Summary
**It force-stops and cleans up the current managed ASR process instance without propagating termination errors.**

`KillProcess` performs best-effort termination of the supervised ASR process when one is tracked. It returns immediately if `_process` is null, otherwise attempts recursive kill (`_process.Kill(true)`) and waits up to 5 seconds for exit when still running, logging failures at debug level without rethrowing. In `finally`, it always disposes the process handle and clears supervisor ownership state (`_process = null`, `_ownsProcess = false`).


#### [[AsrProcessSupervisor.KillProcess]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void KillProcess()
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[AsrProcessSupervisor.EnsureServiceReadyAsync]]

