---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# MfaWorkspaceResolver::ExtractNumericSuffixOrder
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs`

## Summary
**It computes a deterministic sort key for MFA workspace folder names based on numeric suffix conventions.**

`ExtractNumericSuffixOrder` maps workspace directory names to sortable numeric ranks. It returns `int.MaxValue` for null/whitespace names, `int.MaxValue - 1` for the canonical `MFA` name, and for `MFA_<n>` parses and returns `<n>` when numeric. Any non-matching/non-numeric pattern also maps to `int.MaxValue`, pushing unknown forms to the end.


#### [[MfaWorkspaceResolver.ExtractNumericSuffixOrder]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int ExtractNumericSuffixOrder(string name)
```

**Called-by <-**
- [[MfaWorkspaceResolver.EnumerateExistingWorkspaceCandidates]]

