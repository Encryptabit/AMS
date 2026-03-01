---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# MfaWorkflow::CopyIfExists
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`


#### [[MfaWorkflow.CopyIfExists]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void CopyIfExists(string sourcePath, string destinationPath)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[MfaWorkflow.RunChapterAsync]]

