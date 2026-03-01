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
  - llm/async
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrProcessSupervisor::WaitForHealthyAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

## Summary
**It waits for the ASR endpoint to become healthy within a bounded startup window.**

`WaitForHealthyAsync` polls ASR readiness until timeout using the supervisor’s `StartupTimeout` and `HealthInterval` settings. In non-Nemo mode it marks state as `Ready` and returns `true` immediately; otherwise it loops, awaiting `IsHealthyAsync(baseUrl, cancellationToken)` and delaying between checks. It returns `true` on first healthy probe, or `false` if the timeout window elapses without success.


#### [[AsrProcessSupervisor.WaitForHealthyAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<bool> WaitForHealthyAsync(string baseUrl, CancellationToken cancellationToken)
```

**Calls ->**
- [[AsrProcessSupervisor.IsHealthyAsync]]

**Called-by <-**
- [[AsrProcessSupervisor.EnsureServiceReadyAsync]]

