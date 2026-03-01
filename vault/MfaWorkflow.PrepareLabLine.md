---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# MfaWorkflow::PrepareLabLine
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`


#### [[MfaWorkflow.PrepareLabLine]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string PrepareLabLine(string text)
```

**Calls ->**
- [[PronunciationHelper.ExtractPronunciationParts]]

**Called-by <-**
- [[MfaWorkflow.PrepareLabLines]]

