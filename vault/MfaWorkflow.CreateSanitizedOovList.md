---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
---
# MfaWorkflow::CreateSanitizedOovList
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`


#### [[MfaWorkflow.CreateSanitizedOovList]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string CreateSanitizedOovList(string mfaRoot, string chapterStem, string rawOovPath)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[MfaWorkflow.RunChapterAsync]]

