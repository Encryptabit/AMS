---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
---
# AsrProcessSupervisor::IsAutoStartDisabled
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

## Summary
**Resolve an environment-driven feature flag that disables automatic ASR service startup/warmup.**

`IsAutoStartDisabled` reads the `DisableAutoStartEnv` environment variable and treats missing/empty values as auto-start enabled (`false`). It then normalizes only by case-insensitive equality checks and returns `true` when the value is exactly `\"1\"`, `\"true\"`, or `\"yes\"`. Any other non-empty token is treated as not disabled.


#### [[AsrProcessSupervisor.IsAutoStartDisabled]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsAutoStartDisabled()
```

**Called-by <-**
- [[AsrProcessSupervisor.EnsureServiceReadyAsync]]
- [[AsrProcessSupervisor.TriggerBackgroundWarmup]]

