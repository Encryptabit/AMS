---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ReviewedStatusService.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 2
tags:
  - method
---
# ReviewedStatusService::EnsureLoaded
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ReviewedStatusService.cs`


#### [[ReviewedStatusService.EnsureLoaded]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void EnsureLoaded()
```

**Calls ->**
- [[ReviewedStatusService.GetCurrentBookId]]
- [[ReviewedStatusService.Load]]

**Called-by <-**
- [[ReviewedStatusService.GetAll]]
- [[ReviewedStatusService.IsReviewed]]
- [[ReviewedStatusService.ResetCurrentBook]]
- [[ReviewedStatusService.SetReviewed]]

