---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/utility
  - llm/error-handling
  - llm/validation
---
# MfaProcessSupervisor::BuildSupervisorScript
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Summary
**Generate the PowerShell supervisor program that bootstraps MFA and processes command payloads over stdin/stdout with ready/exit signaling.**

`BuildSupervisorScript` programmatically emits a complete PowerShell supervisor script via `StringBuilder`, embedding the supplied `bootstrap` block in a here-string and executing it with guarded error handling. The generated script writes a readiness sentinel (`ReadyToken`), then enters a stdin-driven loop that handles `QuitToken`, parses JSON payloads, optionally switches working directory, executes commands with `Invoke-Expression`, and emits structured completion markers using `ExitToken` plus exit code. Both JSON-parse and command-execution failures are caught in-script, written to stderr, and still produce an exit token so the host can deterministically complete each request.


#### [[MfaProcessSupervisor.BuildSupervisorScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildSupervisorScript(string bootstrap)
```

**Called-by <-**
- [[MfaProcessSupervisor.EnsureBootstrapScript]]

