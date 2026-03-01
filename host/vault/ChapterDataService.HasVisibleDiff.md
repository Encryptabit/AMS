---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ChapterDataService.cs"
access_modifier: "private"
complexity: 14
fan_in: 1
fan_out: 1
tags:
  - method
---
# ChapterDataService::HasVisibleDiff
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ChapterDataService.cs`


#### [[ChapterDataService.HasVisibleDiff]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool HasVisibleDiff(HydratedDiff diff, IReadOnlySet<string> ignoredKeys)
```

**Calls ->**
- [[ErrorPatternService.BuildKey]]

**Called-by <-**
- [[ChapterDataService.GetSentencesAsync]]

