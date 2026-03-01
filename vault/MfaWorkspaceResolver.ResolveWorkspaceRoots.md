---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs"
access_modifier: "public"
complexity: 9
fan_in: 1
fan_out: 2
tags:
  - method
---
# MfaWorkspaceResolver::ResolveWorkspaceRoots
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs`


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

