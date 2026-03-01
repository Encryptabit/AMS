---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs"
access_modifier: "public"
complexity: 9
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/data-access
  - llm/error-handling
---
# MfaWorkspaceResolver::ResolveWorkspaceRoots
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs`

## Summary
**It resolves a deterministic list of available MFA workspace roots for parallel or multi-slot usage.**

`ResolveWorkspaceRoots` returns a normalized set of MFA workspace directories sized from `requestedCount` (minimum 1), preferring explicit multi-root configuration from `AMS_MFA_WORKSPACES` when present. Without config, it seeds candidates from existing `My Documents` workspaces (`MFA*`) and then generates additional `MFA_1..N` paths (where `N` is `1` or `max(8, requestedCount)`), de-duplicating case-insensitively. If no candidates exist it falls back to `My Documents\\MFA`, then ensures each selected directory exists via `EnsureWorkspace` and returns up to the generated target count.


#### [[MfaWorkspaceResolver.ResolveWorkspaceRoots]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<string> ResolveWorkspaceRoots(int requestedCount)
```

**Calls ->**
- [[MfaWorkspaceResolver.EnumerateExistingWorkspaceCandidates]]
- [[MfaWorkspaceResolver.ParseWorkspaceList]]

**Called-by <-**
- [[PipelineConcurrencyControl.ResolveWorkspaceRoots]]

