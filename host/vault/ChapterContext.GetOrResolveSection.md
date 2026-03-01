---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs"
access_modifier: "internal"
complexity: 8
fan_in: 2
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ChapterContext::GetOrResolveSection
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs`

## Summary
**Resolves (and memoizes) the best matching book section for a chapter using override and label-based strategies before caller-side auto-detection.**

`GetOrResolveSection` resolves and caches the chapter’s `SectionRange` using a precedence chain: cached `_resolvedSection`, explicit `options.SectionOverride`, then label-based lookup when `TryResolveSectionFromLabels` is enabled. In label mode it iterates candidates from `ChapterLabelResolver.EnumerateLabelCandidates(...)`, first attempting numeric extraction (`TryExtractChapterNumber`) and title-based resolution via `SectionLocator.ResolveSectionByTitle`, then direct label lookup. Successful resolutions are memoized into `_resolvedSection` and logged with stage/context metadata; unresolved paths emit a debug log and return `null`. The method leaves final auto-detection to the caller as documented.


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

