---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs"
access_modifier: "internal"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
---
# ChapterDocuments::SaveChanges
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs`

## Summary
**Persists all chapter document slots by invoking each slot’s save operation.**

`SaveChanges` is a deterministic flush routine that calls `Save()` on every managed document slot in fixed order: transcript, hydrated transcript, anchors, ASR, ASR transcript text, pause adjustments, pause policy, and text grid. It adds no branching, validation, or error handling, delegating dirty-check/write semantics to each `DocumentSlot`. The method serves as the chapter-level persistence commit point across all document artifacts.


#### [[ChapterDocuments.SaveChanges]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal void SaveChanges()
```

**Calls ->**
- [[DocumentSlot_T_.Save]]

**Called-by <-**
- [[MergeTimingsCommand.ExecuteAsync]]
- [[ChapterContext.Save]]

