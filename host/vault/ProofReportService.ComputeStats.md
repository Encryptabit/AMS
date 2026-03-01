---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ProofReportService.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
---
# ProofReportService::ComputeStats
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ProofReportService.cs`


#### [[ProofReportService.ComputeStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private ChapterStats ComputeStats(HydratedTranscript hydrate)
```

**Calls ->**
- [[ProofReportService.FormatPercentage]]

**Called-by <-**
- [[ProofReportService.BuildReport]]

