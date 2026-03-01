---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BlazorWorkspace.cs"
access_modifier: "public"
complexity: 8
fan_in: 3
fan_out: 1
tags:
  - method
---
# BlazorWorkspace::TryGetHydratedTranscript
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BlazorWorkspace.cs`


#### [[BlazorWorkspace.TryGetHydratedTranscript]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.BlazorWorkspace.TryGetHydratedTranscript(System.String,Ams.Core.Artifacts.Hydrate.HydratedTranscript@)">
    <summary>
    Try to load the hydrated transcript for a chapter without changing the current selection.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool TryGetHydratedTranscript(string chapterName, out HydratedTranscript hydrated)
```

**Calls ->**
- [[BlazorWorkspace.OpenChapter]]

**Called-by <-**
- [[CrxService.BuildSeededLegacyEntry]]
- [[ErrorPatternService.AggregatePatterns]]
- [[PolishService.GetCurrentHydratedTranscript]]

