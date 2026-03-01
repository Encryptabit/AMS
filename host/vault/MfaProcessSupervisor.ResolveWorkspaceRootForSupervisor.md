---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/error-handling
---
# MfaProcessSupervisor::ResolveWorkspaceRootForSupervisor
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Summary
**Attempt to resolve the MFA workspace root for supervisor environment setup while degrading gracefully on resolution errors.**

`ResolveWorkspaceRootForSupervisor` is a thin wrapper around `MfaWorkspaceResolver.ResolvePreferredRoot()`, returning its result directly on success. It catches any `Exception`, logs a debug message with `ex.Message` (`"Unable to resolve MFA workspace root for supervisor: {Message}"`), and returns `null` as a non-fatal fallback. This keeps supervisor startup resilient when workspace resolution fails.


#### [[MfaProcessSupervisor.ResolveWorkspaceRootForSupervisor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveWorkspaceRootForSupervisor()
```

**Calls ->**
- [[MfaWorkspaceResolver.ResolvePreferredRoot]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaProcessSupervisor.StartProcessAsync]]

