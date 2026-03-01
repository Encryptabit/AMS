---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs"
access_modifier: "internal"
complexity: 8
fan_in: 2
fan_out: 3
tags:
  - method
---
# ChapterContext::GetOrResolveSection
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs`


#### [[ChapterContext.GetOrResolveSection]]
##### What it does:
<member name="M:Ams.Core.Runtime.Chapter.ChapterContext.GetOrResolveSection(Ams.Core.Runtime.Book.BookIndex,Ams.Core.Services.Alignment.AnchorComputationOptions,System.String,Microsoft.Extensions.Logging.ILogger)">
    <summary>
    Resolve and cache the book section for this chapter (override > label mapping; auto-detect handled by caller).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal SectionRange GetOrResolveSection(BookIndex book, AnchorComputationOptions options, string stage, ILogger logger)
```

**Calls ->**
- [[ChapterLabelResolver.EnumerateLabelCandidates]]
- [[ChapterLabelResolver.TryExtractChapterNumber]]
- [[SectionLocator.ResolveSectionByTitle]]

**Called-by <-**
- [[AnchorComputeService.ComputeAnchorsAsync]]
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]

