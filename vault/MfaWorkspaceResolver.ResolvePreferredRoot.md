---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs"
access_modifier: "public"
complexity: 7
fan_in: 3
fan_out: 4
tags:
  - method
---
# MfaWorkspaceResolver::ResolvePreferredRoot
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs`


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

