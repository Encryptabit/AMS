---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/entry-point
  - llm/utility
  - llm/data-access
  - llm/error-handling
---
# ChapterManager::DeallocateAll
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Saves and deallocates every cached chapter context, then resets all chapter cache tracking structures.**

`DeallocateAll` flushes all cached `ChapterContext` instances by iterating `_cache.Values`, persisting each context (`context.Save()`), and releasing associated audio buffers (`context.Audio.DeallocateAll()`). After cleanup it clears cache and LRU state (`_cache`, `_usageNodes`, `_usageOrder`) and emits a debug log with final cache counts. This is the manager-wide teardown path for chapter runtime state.


#### [[ChapterManager.DeallocateAll]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void DeallocateAll()
```

**Calls ->**
- [[AudioBufferManager.DeallocateAll]]
- [[ChapterContext.Save]]

**Called-by <-**
- [[BookManager.Deallocate]]
- [[BookManager.DeallocateAll]]

