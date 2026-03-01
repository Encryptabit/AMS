---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ValidationMetricsService.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 0
tags:
  - method
---
# ValidationMetricsService::FormatPercentage
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ValidationMetricsService.cs`


#### [[ValidationMetricsService.FormatPercentage]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.ValidationMetricsService.FormatPercentage(System.Double)">
    <summary>
    Format a decimal WER value as percentage string (e.g., "2.48%").
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatPercentage(double value)
```

**Called-by <-**
- [[ValidationMetricsService.ComputeBookOverview]]
- [[ValidationMetricsService.ComputeChapterMetrics]]

