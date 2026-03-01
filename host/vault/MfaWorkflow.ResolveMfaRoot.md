---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# MfaWorkflow::ResolveMfaRoot
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`

## Summary
**It resolves the effective MFA workspace root path, optionally honoring an explicit override.**

`ResolveMfaRoot` is a pass-through helper that delegates MFA workspace resolution to `MfaWorkspaceResolver.ResolvePreferredRoot(overrideRoot)` and returns the resolved root path. It centralizes root selection logic behind a local method for workflow callers.


#### [[MfaWorkflow.ResolveMfaRoot]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveMfaRoot(string overrideRoot = null)
```

**Calls ->**
- [[MfaWorkspaceResolver.ResolvePreferredRoot]]

**Called-by <-**
- [[MfaWorkflow.RunChapterAsync]]

