---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BlazorWorkspace.cs"
access_modifier: "public"
complexity: 8
fan_in: 1
fan_out: 2
tags:
  - method
---
# BlazorWorkspace::SelectChapter
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/BlazorWorkspace.cs`


#### [[BlazorWorkspace.SelectChapter]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.BlazorWorkspace.SelectChapter(System.String)">
    <summary>
    Selects a chapter by name, opening its context handle.
    Chapters remain cached until workspace is disposed (LRU managed by ChapterManager).
    </summary>
    <param name="chapterName">The chapter name (display title) to select.</param>
    <returns>True if chapter was opened successfully.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool SelectChapter(string chapterName)
```

**Calls ->**
- [[BlazorWorkspace.OpenChapter]]
- [[BlazorWorkspace.SavePersistedState]]

**Called-by <-**
- [[BlazorWorkspace.LoadPersistedState]]

