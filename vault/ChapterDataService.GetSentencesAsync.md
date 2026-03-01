---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ChapterDataService.cs"
access_modifier: "public"
complexity: 3
fan_in: 2
fan_out: 2
tags:
  - method
---
# ChapterDataService::GetSentencesAsync
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/ChapterDataService.cs`


#### [[ChapterDataService.GetSentencesAsync]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.ChapterDataService.GetSentencesAsync(System.String,System.Collections.Generic.IReadOnlySet{System.String})">
    <summary>
    Gets the list of sentences for a chapter from the HydratedTranscript.
    </summary>
    <param name="chapterName">The name of the chapter to load.</param>
    <returns>A list of sentences with timing information.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<List<SentenceViewModel>> GetSentencesAsync(string chapterName, IReadOnlySet<string> ignoredKeys = null)
```

**Calls ->**
- [[ChapterDataService.BuildDiffHtml]]
- [[ChapterDataService.HasVisibleDiff]]

**Called-by <-**
- [[ChapterDataService.GetChapterDurationAsync]]
- [[ChapterDataService.GetSentenceAsync]]

