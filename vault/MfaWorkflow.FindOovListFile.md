---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# MfaWorkflow::FindOovListFile
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`


#### [[MfaWorkflow.FindOovListFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FindOovListFile(string directory)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[MfaWorkflow.RunChapterAsync]]

