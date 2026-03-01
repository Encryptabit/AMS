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
# ChapterManager::RemoveUsageNode
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Deletes a chapter’s usage-tracking node from LRU bookkeeping structures.**

`RemoveUsageNode` removes a chapter’s LRU tracking entry if present. It looks up `chapterId` in `_usageNodes`, and on hit removes the linked-list node from `_usageOrder` and deletes the dictionary mapping. Missing IDs are treated as no-op, keeping cache eviction metadata consistent with deallocation operations.


#### [[ChapterManager.RemoveUsageNode]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void RemoveUsageNode(string chapterId)
```

**Called-by <-**
- [[ChapterManager.Deallocate]]

