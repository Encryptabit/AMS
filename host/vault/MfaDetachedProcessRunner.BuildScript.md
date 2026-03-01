---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# MfaDetachedProcessRunner::BuildScript
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs`

## Summary
**It generates a self-contained PowerShell script that bootstraps MFA, optionally changes directory, runs the requested command, and returns a deterministic exit code.**

`BuildScript` assembles the full PowerShell script text for detached MFA execution using a `StringBuilder`. It injects the bootstrap sequence from `MfaProcessSupervisor.ResolveBootstrapSequence()`, adds guarded `try/catch` blocks for bootstrap execution and optional `Set-Location`, and escapes single quotes in both `workingDirectory` and `command` before `Invoke-Expression`. The generated script normalizes process termination by setting `$global:LASTEXITCODE`, computing `$exitCode` on success/failure, and ending with `exit $exitCode`.


#### [[MfaDetachedProcessRunner.BuildScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildScript(string command, string workingDirectory)
```

**Calls ->**
- [[MfaProcessSupervisor.ResolveBootstrapSequence]]

**Called-by <-**
- [[MfaDetachedProcessRunner.WriteScript]]

