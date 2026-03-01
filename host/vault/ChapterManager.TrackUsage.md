---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# ChapterManager::TrackUsage
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Updates chapter usage tracking so recent accesses are reflected in LRU cache order.**

`TrackUsage` maintains LRU ordering metadata for cached chapter contexts. If `chapterId` already has a node in `_usageNodes`, it moves that node to the tail of `_usageOrder` (most recently used); otherwise it appends a new node and records it in `_usageNodes`. This keeps dictionary and linked-list state synchronized for eviction logic in `EnsureCapacity`.


#### [[ChapterManager.TrackUsage]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void TrackUsage(string chapterId)
```

**Called-by <-**
- [[ChapterManager.GetOrCreate]]

