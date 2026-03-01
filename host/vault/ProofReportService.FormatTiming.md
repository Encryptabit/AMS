---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ProofReportService.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 0
tags:
  - method
---
# ProofReportService::FormatTiming
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ProofReportService.cs`


#### [[ProofReportService.FormatTiming]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.ProofReportService.FormatTiming(System.Double,System.Double)">
    <summary>
    Format timing as "870.530s -> 871.050s (delta 0.520s)".
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatTiming(double start, double end)
```

**Called-by <-**
- [[ProofReportService.BuildParagraphReport]]
- [[ProofReportService.BuildSentenceReport]]

