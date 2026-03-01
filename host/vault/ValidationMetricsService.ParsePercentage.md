---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ValidationMetricsService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
---
# ValidationMetricsService::ParsePercentage
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ValidationMetricsService.cs`


#### [[ValidationMetricsService.ParsePercentage]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.ValidationMetricsService.ParsePercentage(System.String)">
    <summary>
    Parse a percentage string back to decimal value.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double ParsePercentage(string percentage)
```

**Called-by <-**
- [[ValidationMetricsService.ComputeBookOverview]]

