---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/async
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# MfaProcessSupervisor::WaitForReadyAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Summary
**Block asynchronously until the MFA supervisor emits its explicit ready marker, with live output logging and failure on premature termination.**

`WaitForReadyAsync` reads from `_lineChannel` (throwing if it is not initialized) and asynchronously waits for lines using `WaitToReadAsync(cancellationToken)` while draining with `TryRead`. It treats a `StdOut` line exactly equal to `ReadyToken` (ordinal comparison) as the startup handshake, logs `"MFA environment ready"`, and returns. Until then it logs stderr lines as `mfa!` and stdout lines as `mfa>` for diagnostics. If the channel completes before the ready token arrives, it throws `InvalidOperationException` indicating readiness was never signaled.


#### [[MfaProcessSupervisor.WaitForReadyAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task WaitForReadyAsync(CancellationToken cancellationToken)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[MfaProcessSupervisor.StartProcessAsync]]

