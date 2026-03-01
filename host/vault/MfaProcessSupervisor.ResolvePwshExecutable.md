---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "internal"
complexity: 3
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
---
# MfaProcessSupervisor::ResolvePwshExecutable
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Summary
**Choose the PowerShell executable path/name for MFA process launches, with an environment-variable override.**

`ResolvePwshExecutable` resolves the PowerShell host binary by first checking `AMS_MFA_PWSH` in the environment and returning that override when non-empty. If unset, it branches on `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)` to return `"pwsh.exe"` on Windows and `"pwsh"` elsewhere. The method is deterministic, side-effect free, and provides platform-default fallback behavior.


#### [[MfaProcessSupervisor.ResolvePwshExecutable]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static string ResolvePwshExecutable()
```

**Called-by <-**
- [[MfaDetachedProcessRunner.RunAsync]]
- [[MfaProcessSupervisor.StartProcessAsync]]

