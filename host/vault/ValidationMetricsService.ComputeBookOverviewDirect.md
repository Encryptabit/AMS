---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ValidationMetricsService.cs"
access_modifier: "public"
complexity: 8
fan_in: 0
fan_out: 3
tags:
  - method
---
# ValidationMetricsService::ComputeBookOverviewDirect
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ValidationMetricsService.cs`


#### [[ValidationMetricsService.ComputeBookOverviewDirect]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.ValidationMetricsService.ComputeBookOverviewDirect(Ams.Workstation.Server.Services.BlazorWorkspace)">
    <summary>
    Compute full book overview by reading hydrate files directly from disk.
    Much faster than going through full chapter context loading.
    </summary>
    <param name="workspace">The workspace to get chapter info from.</param>
    <returns>Book overview with all chapter metrics, or null if workspace not initialized.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookOverview ComputeBookOverviewDirect(BlazorWorkspace workspace)
```

**Calls ->**
- [[BlazorWorkspace.GetStemForChapter]]
- [[ValidationMetricsService.ComputeBookOverview]]
- [[ValidationMetricsService.ComputeChapterMetrics]]

