---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ValidationMetricsService.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
---
# ValidationMetricsService::ComputeChapterMetrics
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ValidationMetricsService.cs`


#### [[ValidationMetricsService.ComputeChapterMetrics]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.ValidationMetricsService.ComputeChapterMetrics(Ams.Core.Artifacts.Hydrate.HydratedTranscript)">
    <summary>
    Compute chapter metrics from HydratedTranscript.
    </summary>
    <param name="hydrate">The hydrated transcript to analyze.</param>
    <returns>Chapter metrics with sentence/paragraph counts and WER averages.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ChapterMetrics ComputeChapterMetrics(HydratedTranscript hydrate)
```

**Calls ->**
- [[ValidationMetricsService.FormatPercentage]]
- [[ValidationMetricsService.IsSentenceFlagged]]

**Called-by <-**
- [[ValidationMetricsService.ComputeBookOverviewDirect]]

