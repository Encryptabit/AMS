---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "internal"
complexity: 8
fan_in: 2
fan_out: 7
tags:
  - method
  - llm/async
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrProcessSupervisor::EnsureServiceReadyAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

## Summary
**It validates and, when permitted, bootstraps the Nemo ASR service so callers can rely on a healthy endpoint.**

`EnsureServiceReadyAsync` ensures ASR availability for `baseUrl` by first checking whether Nemo mode is enabled, then probing service health (`IsHealthyAsync`) before attempting process control. If the endpoint is unhealthy, it rejects non-local targets and auto-start-disabled configurations with faulted state and `InvalidOperationException`. For local managed scenarios, it can restart a stale existing process (`KillProcess`), start a new one (`StartManagedProcess`), and wait for readiness (`WaitForHealthyAsync`), failing on timeout. Throughout the flow it updates supervisor state (`Ready`/`Starting`/`Faulted`) and records diagnostics via `Log.Debug`.


#### [[AsrProcessSupervisor.EnsureServiceReadyAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static Task EnsureServiceReadyAsync(string baseUrl, CancellationToken cancellationToken)
```

**Calls ->**
- [[AsrProcessSupervisor.IsAutoStartDisabled]]
- [[AsrProcessSupervisor.IsHealthyAsync]]
- [[AsrProcessSupervisor.IsLocalBaseUrl]]
- [[AsrProcessSupervisor.KillProcess]]
- [[AsrProcessSupervisor.StartManagedProcess]]
- [[AsrProcessSupervisor.WaitForHealthyAsync]]
- [[Log.Debug]]

**Called-by <-**
- [[GenerateTranscriptCommand.RunNemoAsync]]
- [[AsrProcessSupervisor.TriggerBackgroundWarmup]]

