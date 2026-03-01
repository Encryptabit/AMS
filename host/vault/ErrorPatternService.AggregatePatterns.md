---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ErrorPatternService.cs"
access_modifier: "public"
complexity: 7
fan_in: 0
fan_out: 3
tags:
  - method
---
# ErrorPatternService::AggregatePatterns
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ErrorPatternService.cs`


#### [[ErrorPatternService.AggregatePatterns]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.ErrorPatternService.AggregatePatterns(System.Collections.Generic.IReadOnlySet{System.String})">
    <summary>
    Aggregate error patterns across all chapters in the workspace.
    </summary>
    <param name="ignoredKeys">Set of pattern keys to mark as ignored.</param>
    <returns>Aggregated patterns sorted by occurrence count (descending).</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ErrorPatternsResult AggregatePatterns(IReadOnlySet<string> ignoredKeys = null)
```

**Calls ->**
- [[BlazorWorkspace.TryGetHydratedTranscript]]
- [[ErrorPatternService.BuildKey]]
- [[ErrorPatternService.ExtractPatterns]]

