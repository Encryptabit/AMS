---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 3
tags:
  - method
  - llm/factory
  - llm/utility
  - llm/di
  - llm/data-access
  - llm/error-handling
---
# ChapterManager::GetOrCreate
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Retrieves a cached chapter context or creates one, while maintaining usage tracking and cache-capacity enforcement.**

`GetOrCreate` implements cache-backed chapter context resolution keyed by `descriptor.ChapterId`. On cache miss, it constructs `new ChapterContext(_bookContext, descriptor)`, stores it, updates LRU usage (`TrackUsage`), logs creation, and invokes `EnsureCapacity` which may evict/safely flush older contexts. On cache hit, it only refreshes usage tracking and logs reuse. It always returns the resolved `ChapterContext` instance.


#### [[ChapterManager.GetOrCreate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private ChapterContext GetOrCreate(ChapterDescriptor descriptor)
```

**Calls ->**
- [[Log.Debug]]
- [[ChapterManager.EnsureCapacity]]
- [[ChapterManager.TrackUsage]]

**Called-by <-**
- [[ChapterManager.Load]]
- [[ChapterManager.Load_2]]

