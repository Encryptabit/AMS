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
  - llm/error-handling
---
# MfaProcessSupervisor::PumpAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Summary
**Continuously forward process stdout/stderr lines into the supervisor’s channel and coordinate channel completion when all pumps stop.**

`PumpAsync` streams lines from a given `StreamReader` into the shared `_lineChannel` as `ProcessLine(kind, line)` records, and throws immediately if the channel is not initialized. It loops until cancellation or EOF (`ReadLineAsync` returns `null`), writing each line with cancellation-aware `channel.Writer.WriteAsync`. `OperationCanceledException` is treated as expected shutdown, while other exceptions are caught and logged via `Log.Debug("MFA output pump faulted: {0}", ex)`. In `finally`, it decrements `_activePumpCount` with `Interlocked.Decrement` and completes the channel writer once the last pump exits.


#### [[MfaProcessSupervisor.PumpAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task PumpAsync(StreamReader reader, MfaProcessSupervisor.StreamKind kind, CancellationToken cancellationToken)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[MfaProcessSupervisor.StartProcessAsync]]

