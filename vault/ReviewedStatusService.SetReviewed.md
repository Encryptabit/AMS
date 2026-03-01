---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ReviewedStatusService.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 2
tags:
  - method
---
# ReviewedStatusService::SetReviewed
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ReviewedStatusService.cs`


#### [[ReviewedStatusService.SetReviewed]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SetReviewed(string chapterName, bool reviewed)
```

**Calls ->**
- [[ReviewedStatusService.EnsureLoaded]]
- [[ReviewedStatusService.Save]]

**Called-by <-**
- [[PolishVerificationService.SyncToProofAsync]]

