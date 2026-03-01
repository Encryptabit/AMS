---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "internal"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# MfaProcessSupervisor::RegisterForShutdown
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Summary
**Register global process-exit and cancel-key handlers so MFA supervisor resources are shut down on application termination.**

`RegisterForShutdown` wires process-lifetime hooks to centralized cleanup by subscribing to `AppDomain.CurrentDomain.ProcessExit` and `Console.CancelKeyPress`. Both handlers use discard-parameter lambdas that invoke `Shutdown()` directly, so termination and Ctrl+C paths share the same teardown logic. The method is synchronous and contains only event registration, with no branching or state checks.


#### [[MfaProcessSupervisor.RegisterForShutdown]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static void RegisterForShutdown()
```

**Calls ->**
- [[MfaProcessSupervisor.Shutdown]]

**Called-by <-**
- [[Program.Main]]

