---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 1
tags:
  - method
---
# MfaWorkspaceResolver::EnumerateExistingWorkspaceCandidates
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs`


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

