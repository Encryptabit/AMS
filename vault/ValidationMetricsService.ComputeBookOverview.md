---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ValidationMetricsService.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 2
tags:
  - method
---
# ValidationMetricsService::ComputeBookOverview
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ValidationMetricsService.cs`


#### [[ValidationMetricsService.ComputeBookOverview]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.ValidationMetricsService.ComputeBookOverview(System.Collections.Generic.IEnumerable{System.ValueTuple{System.String,Ams.Workstation.Server.Services.ChapterMetrics}})">
    <summary>
    Compute book-wide aggregated metrics from all chapters.
    </summary>
    <param name="chapters">List of chapter names with their metrics.</param>
    <returns>Book overview with aggregated statistics.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookOverview ComputeBookOverview(IEnumerable<(string ChapterName, ChapterMetrics Metrics)> chapters)
```

**Calls ->**
- [[ValidationMetricsService.FormatPercentage]]
- [[ValidationMetricsService.ParsePercentage]]

**Called-by <-**
- [[ValidationMetricsService.ComputeBookOverviewDirect]]

