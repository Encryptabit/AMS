---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/error-handling
  - llm/validation
---
# ChapterManager::EnsureCapacity
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Maintains cache size bounds by evicting and flushing least-recently-used chapter contexts.**

`EnsureCapacity` enforces the chapter context cache limit by evicting least-recently-used entries while `_cache.Count > MaxCachedContexts`. It repeatedly removes the LRU key from `_usageOrder`/`_usageNodes`, then removes the corresponding context from `_cache`; on successful removal it persists state (`context.Save()`), deallocates audio buffers (`context.Audio.DeallocateAll()`), and logs eviction details. The loop continues until capacity constraints are satisfied or no LRU node remains.


#### [[ChapterManager.EnsureCapacity]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void EnsureCapacity()
```

**Calls ->**
- [[Log.Debug]]
- [[AudioBufferManager.DeallocateAll]]
- [[ChapterContext.Save]]

**Called-by <-**
- [[ChapterManager.GetOrCreate]]

