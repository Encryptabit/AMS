---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ProofReportService.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 3
tags:
  - method
---
# ProofReportService::BuildReport
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ProofReportService.cs`


#### [[ProofReportService.BuildReport]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.ProofReportService.BuildReport(System.String,Ams.Core.Artifacts.Hydrate.HydratedTranscript)">
    <summary>
    Build a complete chapter report from HydratedTranscript.
    </summary>
    <param name="chapterName">Display name for the chapter.</param>
    <param name="hydrate">The hydrated transcript to build report from.</param>
    <returns>Complete chapter report with sentences, paragraphs, and statistics.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ChapterReport BuildReport(string chapterName, HydratedTranscript hydrate)
```

**Calls ->**
- [[ProofReportService.BuildParagraphReport]]
- [[ProofReportService.BuildSentenceReport]]
- [[ProofReportService.ComputeStats]]

