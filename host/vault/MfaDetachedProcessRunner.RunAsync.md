---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 8
tags:
  - method
  - llm/async
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# MfaDetachedProcessRunner::RunAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs`

## Summary
**It runs an MFA subcommand in an isolated PowerShell process, captures execution output, and returns a structured command result with robust cancellation and cleanup behavior.**

`RunAsync` validates `subcommand`, composes the `mfa` CLI string via `BuildCommand`, normalizes `workingDirectory`, and materializes a temporary PowerShell script with `WriteScript` to execute detached MFA work. It launches `pwsh` from `MfaProcessSupervisor.ResolvePwshExecutable()` with redirected stdout/stderr, optionally resolves `workspaceRoot` through `MfaWorkspaceResolver.ResolvePreferredRoot()` (logging failures with `Log.Debug`) and exports it as `MFA_ROOT_DIR`. Output streams are drained concurrently by `PumpStreamAsync`, cancellation registers a best-effort recursive `process.Kill(true)`, and the method returns `MfaCommandResult` with exit code and captured logs while guaranteeing temp-script cleanup in `finally` via `TryDeleteScript`.


#### [[MfaDetachedProcessRunner.RunAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<MfaCommandResult> RunAsync(string subcommand, string args, string workingDirectory, string workspaceRoot, CancellationToken cancellationToken)
```

**Calls ->**
- [[MfaDetachedProcessRunner.BuildCommand]]
- [[MfaDetachedProcessRunner.NormalizeWorkingDirectory]]
- [[MfaDetachedProcessRunner.PumpStreamAsync]]
- [[MfaDetachedProcessRunner.TryDeleteScript]]
- [[MfaDetachedProcessRunner.WriteScript]]
- [[MfaWorkspaceResolver.ResolvePreferredRoot]]
- [[MfaProcessSupervisor.ResolvePwshExecutable]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaService.RunCommandAsync]]

