---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "internal"
complexity: 2
fan_in: 1
fan_out: 5
tags:
  - method
  - llm/async
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# MfaProcessSupervisor::RunAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Summary
**Execute one MFA subcommand through the warm supervised process and return structured command output and exit status.**

`RunAsync` validates `subcommand` (`ArgumentException` on null/whitespace), then acquires the static `CommandGate` semaphore with the caller’s cancellation token to enforce single-command execution. Inside `try/finally`, it awaits `EnsureStartedAsync`, builds the shell command, normalizes the working directory, serializes a `Payload` to JSON, logs it, and writes/flushed it to the supervised process stdin. It then collects stdout/stderr lines, awaits `WaitForCommandCompletionAsync` for the exit code, and returns `new MfaCommandResult(command, exitCode, stdout, stderr)`; the semaphore is always released in `finally`.


#### [[MfaProcessSupervisor.RunAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static Task<MfaCommandResult> RunAsync(string subcommand, string args, string workingDirectory, CancellationToken cancellationToken)
```

**Calls ->**
- [[MfaProcessSupervisor.BuildCommand]]
- [[MfaProcessSupervisor.EnsureStartedAsync]]
- [[MfaProcessSupervisor.NormalizeWorkingDirectory]]
- [[MfaProcessSupervisor.WaitForCommandCompletionAsync]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaService.RunCommandAsync]]

