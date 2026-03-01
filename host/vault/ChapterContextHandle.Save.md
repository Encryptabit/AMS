---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs"
access_modifier: "public"
complexity: 1
fan_in: 7
fan_out: 2
tags:
  - method
  - llm/entry-point
  - llm/data-access
  - llm/utility
---
# ChapterContextHandle::Save
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs`

## Summary
**Commits both chapter and book context changes through a single handle-level save operation.**

`ChapterContextHandle.Save` is a two-step persistence wrapper that flushes chapter-level and book-level pending changes. It calls `_chapterContext.Save()` first, then `_bookContext.Save()`, with no local branching, retries, or exception handling. This centralizes save orchestration for the handle’s paired contexts.


#### [[ChapterContextHandle.Save]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Save()
```

**Calls ->**
- [[BookContext.Save]]
- [[ChapterContext.Save]]

**Called-by <-**
- [[AlignCommand.CreateAnchors]]
- [[AlignCommand.CreateHydrateTx]]
- [[AlignCommand.CreateTranscriptIndex]]
- [[AsrCommand.Create]]
- [[ValidateCommand.CreateReportCommand]]
- [[ValidateTimingSession.LoadSessionContextAsync]]
- [[PipelineService.RunChapterAsync]]

