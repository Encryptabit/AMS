---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# MfaWorkspaceResolver::EnumerateExistingWorkspaceCandidates
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs`

## Summary
**It discovers and deterministically orders existing MFA workspace directory candidates under a documents root.**

`EnumerateExistingWorkspaceCandidates` probes `documentsRoot` for top-level directories matching `MFA*` and suppresses filesystem errors by falling back to an empty sequence. It keeps only names equal to `MFA` or starting with `MFA_` (case-insensitive), then orders results first by `ExtractNumericSuffixOrder(...)` and then lexicographically with `StringComparer.OrdinalIgnoreCase`. The returned sequence is suitable for deterministic workspace selection in callers.


#### [[MfaWorkspaceResolver.EnumerateExistingWorkspaceCandidates]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<string> EnumerateExistingWorkspaceCandidates(string documentsRoot)
```

**Calls ->**
- [[MfaWorkspaceResolver.ExtractNumericSuffixOrder]]

**Called-by <-**
- [[MfaWorkspaceResolver.ResolvePreferredRoot]]
- [[MfaWorkspaceResolver.ResolveWorkspaceRoots]]

