---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ChapterManager::Load
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterManager.cs`

## Summary
**Loads a chapter context at a specific index while updating cursor state and using cached context retrieval.**

`Load(int index)` resolves a chapter context by descriptor index with bounds validation and cache-backed creation/reuse. It uses an unsigned range check (`(uint)index >= (uint)_descriptors.Count`) to throw `ArgumentOutOfRangeException` on invalid input, updates `_cursor` to the selected index, then returns `GetOrCreate(_descriptors[index])`. This method is the index-based navigation entry used by next/previous traversal.


#### [[ChapterManager.Load]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ChapterContext Load(int index)
```

**Calls ->**
- [[ChapterManager.GetOrCreate]]

**Called-by <-**
- [[ChapterManager.TryMoveNext]]
- [[ChapterManager.TryMovePrevious]]

