---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/data-access
  - llm/validation
---
# MfaProcessSupervisor::EnsureBootstrapScript
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Summary
**Guarantee a valid on-disk bootstrap PowerShell script exists and cache its path for process startup.**

`EnsureBootstrapScript` lazily initializes `_scriptPath` for the MFA supervisor bootstrap script. It first short-circuits when `_scriptPath` is already set and the file still exists, avoiding regeneration. On cache miss, it gets the bootstrap command sequence via `ResolveBootstrapSequence()`, renders the PowerShell wrapper with `BuildSupervisorScript(...)`, writes it to `Path.Combine(Path.GetTempPath(), "ams-mfa-supervisor.ps1")` using UTF-8, and stores that path in `_scriptPath`.


#### [[MfaProcessSupervisor.EnsureBootstrapScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void EnsureBootstrapScript()
```

**Calls ->**
- [[MfaProcessSupervisor.BuildSupervisorScript]]
- [[MfaProcessSupervisor.ResolveBootstrapSequence]]

**Called-by <-**
- [[MfaProcessSupervisor.StartProcessAsync]]

