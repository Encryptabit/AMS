---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# MfaProcessSupervisor::IsProcessRunning
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Summary
**Safely determine whether a `Process` instance currently represents a running process.**

`IsProcessRunning` is a defensive state probe that returns `true` only when `process` is non-null and `HasExited` is `false` (property pattern `process is { HasExited: false }`). It wraps the check in a `try/catch` for `InvalidOperationException`, returning `false` when the `Process` instance is not properly associated with a live OS process. This avoids teardown-time exceptions when process state is unstable.


#### [[MfaProcessSupervisor.IsProcessRunning]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsProcessRunning(Process process)
```

**Called-by <-**
- [[MfaProcessSupervisor.TearDownProcess]]

