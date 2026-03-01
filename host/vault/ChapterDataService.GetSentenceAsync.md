---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/ChapterDataService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
---
# ChapterDataService::GetSentenceAsync
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/ChapterDataService.cs`


#### [[ChapterDataService.GetSentenceAsync]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.ChapterDataService.GetSentenceAsync(System.String,System.Int32)">
    <summary>
    Gets a specific sentence by ID.
    </summary>
    <param name="chapterName">The chapter name.</param>
    <param name="sentenceId">The sentence ID.</param>
    <returns>The sentence, or null if not found.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<SentenceViewModel> GetSentenceAsync(string chapterName, int sentenceId)
```

**Calls ->**
- [[ChapterDataService.GetSentencesAsync]]

