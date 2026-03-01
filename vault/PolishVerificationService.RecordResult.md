---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishVerificationService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
---
# PolishVerificationService::RecordResult
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishVerificationService.cs`


#### [[PolishVerificationService.RecordResult]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PolishVerificationService.RecordResult(System.String,Ams.Workstation.Server.Services.RevalidationResult)">
    <summary>
    Records a revalidation result in the in-memory history for a chapter.
    </summary>
    <param name="chapterStem">The chapter stem identifier.</param>
    <param name="result">The revalidation result to record.</param>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void RecordResult(string chapterStem, RevalidationResult result)
```

