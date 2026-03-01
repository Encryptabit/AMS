---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs"
access_modifier: "public"
complexity: 7
fan_in: 3
fan_out: 4
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# MfaWorkspaceResolver::ResolvePreferredRoot
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs`

## Summary
**It selects and ensures the effective MFA workspace directory from overrides, environment configuration, or document-folder fallbacks.**

`ResolvePreferredRoot` resolves the MFA workspace root using a strict precedence chain: explicit `overrideRoot`, `MFA_ROOT_DIR`, `AMS_MFA_WORKSPACE`, first entry from `AMS_MFA_WORKSPACES`, then a discovered existing workspace under `My Documents` (`MFA`/`MFA_*`), and finally `My Documents\\MFA`. Candidate paths are normalized through `TryNormalizePath`, and every selected path is materialized/normalized via `EnsureWorkspace` before return. If `My Documents` cannot be resolved, it throws `InvalidOperationException`.


#### [[MfaWorkspaceResolver.ResolvePreferredRoot]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string ResolvePreferredRoot(string overrideRoot = null)
```

**Calls ->**
- [[MfaWorkspaceResolver.EnsureWorkspace]]
- [[MfaWorkspaceResolver.EnumerateExistingWorkspaceCandidates]]
- [[MfaWorkspaceResolver.ParseWorkspaceList]]
- [[MfaWorkspaceResolver.TryNormalizePath]]

**Called-by <-**
- [[MfaDetachedProcessRunner.RunAsync]]
- [[MfaWorkflow.ResolveMfaRoot]]
- [[MfaProcessSupervisor.ResolveWorkspaceRootForSupervisor]]

