---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ErrorPatternService.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 2
tags:
  - method
---
# ErrorPatternService::AggregatePatternsForChapter
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ErrorPatternService.cs`


#### [[ErrorPatternService.AggregatePatternsForChapter]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.ErrorPatternService.AggregatePatternsForChapter(System.String,Ams.Core.Artifacts.Hydrate.HydratedTranscript,System.Collections.Generic.IReadOnlySet{System.String})">
    <summary>
    Aggregate error patterns for a single chapter.
    </summary>
    <param name="chapterTitle">Chapter display name for pattern examples.</param>
    <param name="hydrate">The hydrated transcript to extract patterns from.</param>
    <param name="ignoredKeys">Set of pattern keys to mark as ignored.</param>
    <returns>Aggregated patterns sorted by occurrence count (descending).</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ErrorPatternsResult AggregatePatternsForChapter(string chapterTitle, HydratedTranscript hydrate, IReadOnlySet<string> ignoredKeys = null)
```

**Calls ->**
- [[ErrorPatternService.BuildKey]]
- [[ErrorPatternService.ExtractPatterns]]

