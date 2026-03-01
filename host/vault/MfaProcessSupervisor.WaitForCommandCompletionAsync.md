---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/async
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# MfaProcessSupervisor::WaitForCommandCompletionAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Summary
**Read supervised process output until an explicit exit marker is received, while collecting stdout/stderr content and returning the command exit code.**

`WaitForCommandCompletionAsync` consumes `ProcessLine` items from `_lineChannel` (throwing `InvalidOperationException` if uninitialized) and loops with `WaitToReadAsync(cancellationToken)` plus `TryRead` to drain buffered lines. `StdOut` lines are checked for the `ExitToken` prefix; when found, it parses the trailing numeric exit code and returns it, or returns `-1` if parsing fails. Non-token stdout lines are appended to the provided `stdout` list, and `StdErr` lines are appended to `stderr`. If the channel completes before any exit token is observed, it throws `InvalidOperationException` indicating unexpected supervisor termination.


#### [[MfaProcessSupervisor.WaitForCommandCompletionAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<int> WaitForCommandCompletionAsync(List<string> stdout, List<string> stderr, CancellationToken cancellationToken)
```

**Called-by <-**
- [[MfaProcessSupervisor.RunAsync]]

