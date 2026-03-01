---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "internal"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/error-handling
  - llm/validation
---
# AsrProcessSupervisor::RegisterForShutdown
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

## Summary
**It registers ASR supervisor shutdown hooks so managed Nemo processes are stopped when the host process terminates.**

`RegisterForShutdown` conditionally wires process-lifecycle cleanup only when Nemo mode is active (`NemoEnabled`). It subscribes `Shutdown()` to both `AppDomain.CurrentDomain.ProcessExit` and `Console.CancelKeyPress`, ensuring managed ASR process teardown on normal exit and Ctrl+C termination paths. If Nemo is disabled, it exits immediately without registering handlers.


#### [[AsrProcessSupervisor.RegisterForShutdown]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static void RegisterForShutdown()
```

**Calls ->**
- [[AsrProcessSupervisor.Shutdown]]

**Called-by <-**
- [[Program.Main]]

