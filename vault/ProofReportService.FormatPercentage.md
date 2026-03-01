---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ProofReportService.cs"
access_modifier: "private"
complexity: 1
fan_in: 3
fan_out: 0
tags:
  - method
---
# ProofReportService::FormatPercentage
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ProofReportService.cs`


#### [[ProofReportService.FormatPercentage]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.ProofReportService.FormatPercentage(System.Double)">
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
- [[ProofReportService.BuildParagraphReport]]
- [[ProofReportService.BuildSentenceReport]]
- [[ProofReportService.ComputeStats]]

