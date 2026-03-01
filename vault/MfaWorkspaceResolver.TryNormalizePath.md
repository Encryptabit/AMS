---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 0
tags:
  - method
---
# MfaWorkspaceResolver::TryNormalizePath
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkspaceResolver.cs`


#### [[MfaWorkspaceResolver.TryNormalizePath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryNormalizePath(string path, out string normalized)
```

**Called-by <-**
- [[MfaWorkspaceResolver.ParseWorkspaceList]]
- [[MfaWorkspaceResolver.ResolvePreferredRoot]]

