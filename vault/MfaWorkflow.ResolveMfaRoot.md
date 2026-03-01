---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# MfaWorkflow::ResolveMfaRoot
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`


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

