---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ChapterDataService.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
---
# ChapterDataService::GetChapterDurationAsync
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ChapterDataService.cs`


#### [[ChapterDataService.GetChapterDurationAsync]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.ChapterDataService.GetChapterDurationAsync(System.String)">
    <summary>
    Gets the total duration of a chapter based on its sentences.
    </summary>
    <param name="chapterName">The chapter name.</param>
    <returns>The total duration in seconds.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<double> GetChapterDurationAsync(string chapterName)
```

**Calls ->**
- [[ChapterDataService.GetSentencesAsync]]

