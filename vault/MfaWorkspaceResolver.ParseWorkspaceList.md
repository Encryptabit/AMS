---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 1
tags:
  - method
---
# MfaWorkspaceResolver::ParseWorkspaceList
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs`


#### [[MfaWorkspaceResolver.ParseWorkspaceList]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<string> ParseWorkspaceList(string raw)
```

**Calls ->**
- [[MfaWorkspaceResolver.TryNormalizePath]]

**Called-by <-**
- [[MfaWorkspaceResolver.ResolvePreferredRoot]]
- [[MfaWorkspaceResolver.ResolveWorkspaceRoots]]

