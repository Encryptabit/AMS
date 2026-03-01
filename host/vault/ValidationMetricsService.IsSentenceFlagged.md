---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ValidationMetricsService.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
---
# ValidationMetricsService::IsSentenceFlagged
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ValidationMetricsService.cs`


#### [[ValidationMetricsService.IsSentenceFlagged]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.ValidationMetricsService.IsSentenceFlagged(Ams.Core.Artifacts.Hydrate.HydratedSentence)">
    <summary>
    Determines if a sentence is flagged (needs review).
    A sentence is flagged if it has insertions or deletions in its diff,
    or if Status is not "ok" when no diff stats are available.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsSentenceFlagged(HydratedSentence sentence)
```

**Called-by <-**
- [[ValidationMetricsService.ComputeChapterMetrics]]

