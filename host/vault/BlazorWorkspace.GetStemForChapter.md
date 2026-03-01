---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/BlazorWorkspace.cs"
access_modifier: "public"
complexity: 1
fan_in: 3
fan_out: 0
tags:
  - method
---
# BlazorWorkspace::GetStemForChapter
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/BlazorWorkspace.cs`


#### [[BlazorWorkspace.GetStemForChapter]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.BlazorWorkspace.GetStemForChapter(System.String)">
    <summary>
    Gets the WAV stem for a chapter display title.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public string GetStemForChapter(string displayTitle)
```

**Called-by <-**
- [[AudioController.GetChapterRegionAudio]]
- [[BatchOperationService.GetAvailableChapters]]
- [[ValidationMetricsService.ComputeBookOverviewDirect]]

