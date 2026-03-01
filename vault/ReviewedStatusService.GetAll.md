---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ReviewedStatusService.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# ReviewedStatusService::GetAll
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ReviewedStatusService.cs`


#### [[ReviewedStatusService.GetAll]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyDictionary<string, ReviewedEntry> GetAll()
```

**Calls ->**
- [[ReviewedStatusService.EnsureLoaded]]

**Called-by <-**
- [[BookMetadataResetService.ResetCurrentBook]]

