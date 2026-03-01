---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs"
access_modifier: "public"
complexity: 2
fan_in: 3
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# ChapterContextHandle::Dispose
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs`

## Summary
**Releases chapter context resources once by deallocating the associated chapter from the parent book context.**

`Dispose` implements idempotent teardown for `ChapterContextHandle` using an internal `_disposed` guard. On first call, it resolves the current chapter ID from `_chapterContext.Descriptor.ChapterId`, deallocates that chapter via `_bookContext.Chapters.Deallocate(chapterId)`, and marks the handle disposed. Subsequent calls are no-ops.


#### [[ChapterContextHandle.Dispose]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Dispose()
```

**Calls ->**
- [[ChapterManager.Deallocate]]

**Called-by <-**
- [[BlazorWorkspace.Clear]]
- [[BlazorWorkspace.Dispose]]
- [[BlazorWorkspace.SetWorkingDirectory]]

