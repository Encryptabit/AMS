---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ProofReportService.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 2
tags:
  - method
---
# ProofReportService::BuildParagraphReport
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ProofReportService.cs`


#### [[ProofReportService.BuildParagraphReport]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private ParagraphReport BuildParagraphReport(HydratedParagraph paragraph, IReadOnlyList<HydratedSentence> allSentences, HashSet<int> flaggedSentenceIds)
```

**Calls ->**
- [[ProofReportService.FormatPercentage]]
- [[ProofReportService.FormatTiming]]

**Called-by <-**
- [[ProofReportService.BuildReport]]

