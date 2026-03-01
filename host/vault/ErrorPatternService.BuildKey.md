---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ErrorPatternService.cs"
access_modifier: "public"
complexity: 1
fan_in: 4
fan_out: 0
tags:
  - method
---
# ErrorPatternService::BuildKey
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ErrorPatternService.cs`


#### [[ErrorPatternService.BuildKey]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.ErrorPatternService.BuildKey(System.String,System.String,System.String)">
    <summary>
    Build unique pattern key from type and text values.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string BuildKey(string type, string book, string script)
```

**Called-by <-**
- [[ChapterDataService.BuildDiffHtml]]
- [[ChapterDataService.HasVisibleDiff]]
- [[ErrorPatternService.AggregatePatterns]]
- [[ErrorPatternService.AggregatePatternsForChapter]]

