---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "internal"
complexity: 4
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/async
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrProcessSupervisor::TriggerBackgroundWarmup
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

## Summary
**It opportunistically starts a one-at-a-time background ASR readiness workflow for the configured endpoint.**

`TriggerBackgroundWarmup` primes ASR readiness in the background by recording `baseUrl` and launching a single warmup task that calls `EnsureServiceReadyAsync(baseUrl, CancellationToken.None)`. When Nemo is disabled it immediately marks state as `Ready` and returns; when auto-start is disabled it logs and exits without spawning work. Under `ShutdownLock`, it avoids duplicate concurrent warmups by reusing an in-flight `_warmupTask`, and logs any warmup exceptions from the background task.


#### [[AsrProcessSupervisor.TriggerBackgroundWarmup]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static void TriggerBackgroundWarmup(string baseUrl)
```

**Calls ->**
- [[AsrProcessSupervisor.EnsureServiceReadyAsync]]
- [[AsrProcessSupervisor.IsAutoStartDisabled]]
- [[Log.Debug]]

**Called-by <-**
- [[Program.Main]]

