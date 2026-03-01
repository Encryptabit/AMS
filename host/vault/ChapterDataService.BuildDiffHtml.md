---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ChapterDataService.cs"
access_modifier: "private"
complexity: 16
fan_in: 1
fan_out: 1
tags:
  - method
  - danger/high-complexity
---
# ChapterDataService::BuildDiffHtml
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ChapterDataService.cs`

> [!danger] High Complexity (16)
> Cyclomatic complexity: 16. Consider refactoring into smaller methods.


#### [[ChapterDataService.BuildDiffHtml]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildDiffHtml(HydratedDiff diff, IReadOnlySet<string> ignoredKeys)
```

**Calls ->**
- [[ErrorPatternService.BuildKey]]

**Called-by <-**
- [[ChapterDataService.GetSentencesAsync]]

