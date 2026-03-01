---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ErrorPatternService.cs"
access_modifier: "private"
complexity: 9
fan_in: 2
fan_out: 0
tags:
  - method
---
# ErrorPatternService::ExtractPatterns
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ErrorPatternService.cs`


#### [[ErrorPatternService.ExtractPatterns]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.ErrorPatternService.ExtractPatterns(Ams.Core.Artifacts.Hydrate.HydratedDiff)">
    <summary>
    Extract patterns from a single sentence's diff operations.
    </summary>
    <remarks>
    Pattern extraction rules:
    - Consecutive (delete, insert) = substitution (sub)
    - Standalone delete = deletion (del)
    - Standalone insert = insertion (ins)
    - "equal" operations are ignored
    </remarks>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IEnumerable<(string Type, string Book, string Script)> ExtractPatterns(HydratedDiff diff)
```

**Called-by <-**
- [[ErrorPatternService.AggregatePatterns]]
- [[ErrorPatternService.AggregatePatternsForChapter]]

