---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ProofReportService.cs"
access_modifier: "private"
complexity: 12
fan_in: 1
fan_out: 2
tags:
  - method
---
# ProofReportService::BuildSentenceReport
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ProofReportService.cs`


#### [[ProofReportService.BuildSentenceReport]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private SentenceReport BuildSentenceReport(HydratedSentence sentence, Dictionary<int, int> sentenceToParagraph)
```

**Calls ->**
- [[ProofReportService.FormatPercentage]]
- [[ProofReportService.FormatTiming]]

**Called-by <-**
- [[ProofReportService.BuildReport]]

