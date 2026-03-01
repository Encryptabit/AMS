---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "internal"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
---
# MfaProcessSupervisor::ResolveBootstrapSequence
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Summary
**Provide the PowerShell bootstrap command sequence used to initialize the MFA runtime environment.**

`ResolveBootstrapSequence` returns the MFA environment bootstrap script text, preferring an override from `AMS_MFA_BOOTSTRAP` when present. Environment-provided content is normalized by converting CRLF to LF via `Replace("\r\n", "\n")`. If no override is set, it returns a default multi-line PowerShell sequence built with `string.Join(Environment.NewLine, ...)` that invokes conda hook and activates the configured environments.


#### [[MfaProcessSupervisor.ResolveBootstrapSequence]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static string ResolveBootstrapSequence()
```

**Called-by <-**
- [[MfaDetachedProcessRunner.BuildScript]]
- [[MfaProcessSupervisor.EnsureBootstrapScript]]

