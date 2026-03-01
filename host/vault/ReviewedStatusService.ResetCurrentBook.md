---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ReviewedStatusService.cs"
access_modifier: "public"
complexity: 1
fan_in: 2
fan_out: 2
tags:
  - method
---
# ReviewedStatusService::ResetCurrentBook
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ReviewedStatusService.cs`


#### [[ReviewedStatusService.ResetCurrentBook]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void ResetCurrentBook()
```

**Calls ->**
- [[ReviewedStatusService.EnsureLoaded]]
- [[ReviewedStatusService.Save]]

**Called-by <-**
- [[BookMetadataResetService.ResetCurrentBook]]
- [[ReviewedStatusService.ResetAll]]

