---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/data-access
---
# BookManager::DeallocateAll
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`

## Summary
**Persists and deallocates all cached book contexts and their chapter resources, then clears the manager cache.**

`DeallocateAll` flushes every cached `BookContext` by iterating `_cache.Values`, persisting each context (`context.Save()`), and cascading teardown to chapters (`context.Chapters.DeallocateAll()`). It logs each flushed book ID and then clears the cache dictionary via `_cache.Clear()`. This method performs full in-memory context eviction with pre-eviction persistence.


#### [[BookManager.DeallocateAll]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void DeallocateAll()
```

**Calls ->**
- [[Log.Debug]]
- [[BookContext.Save]]
- [[ChapterManager.DeallocateAll]]

