---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ReviewedStatusService.cs"
access_modifier: "private"
complexity: 8
fan_in: 2
fan_out: 1
tags:
  - method
---
# ReviewedStatusService::Save
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ReviewedStatusService.cs`


#### [[ReviewedStatusService.Save]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void Save()
```

**Calls ->**
- [[ReviewedStatusService.GetFilePath]]

**Called-by <-**
- [[ReviewedStatusService.ResetCurrentBook]]
- [[ReviewedStatusService.SetReviewed]]

