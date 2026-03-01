---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/entry-point
  - llm/utility
  - llm/validation
  - llm/data-access
  - llm/error-handling
---
# ChapterManager::Deallocate
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Removes a cached chapter context, saving it and freeing associated audio/cache-tracking resources.**

`Deallocate` evicts a single chapter context from the manager cache by ID, with idempotent no-op behavior for blank IDs or missing entries. On successful `_cache.Remove(chapterId, out var context)`, it persists the context (`context.Save()`), releases audio buffers (`context.Audio.DeallocateAll()`), removes LRU tracking (`RemoveUsageNode(chapterId)`), and logs cache state. This method coordinates cleanup across persistence, resource deallocation, and cache bookkeeping.


#### [[ChapterManager.Deallocate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Deallocate(string chapterId)
```

**Calls ->**
- [[Log.Debug]]
- [[AudioBufferManager.DeallocateAll]]
- [[ChapterContext.Save]]
- [[ChapterManager.RemoveUsageNode]]

**Called-by <-**
- [[ChapterContextHandle.Dispose]]

