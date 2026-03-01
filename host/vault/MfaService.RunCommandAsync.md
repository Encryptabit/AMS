---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaService.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 2
tags:
  - method
  - llm/async
  - llm/utility
---
# MfaService::RunCommandAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaService.cs`

## Summary
**It routes MFA command execution to either the detached runner or the supervised in-process runner based on service configuration.**

`RunCommandAsync` is a dispatch wrapper that selects the MFA execution backend based on the `_useDedicatedProcess` field initialized in the constructor. When enabled, it calls `MfaDetachedProcessRunner.RunAsync(subcommand, args, workingDirectory, _workspaceRoot, cancellationToken)`; otherwise it routes to `MfaProcessSupervisor.RunAsync(subcommand, args, workingDirectory, cancellationToken)`. It returns the selected backend’s `Task<MfaCommandResult>` directly without additional transformation.


#### [[MfaService.RunCommandAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<MfaCommandResult> RunCommandAsync(string subcommand, string args, string workingDirectory, CancellationToken cancellationToken)
```

**Calls ->**
- [[MfaDetachedProcessRunner.RunAsync]]
- [[MfaProcessSupervisor.RunAsync]]

**Called-by <-**
- [[MfaService.AddWordsAsync]]
- [[MfaService.AlignAsync]]
- [[MfaService.GeneratePronunciationsAsync]]
- [[MfaService.ValidateAsync]]

