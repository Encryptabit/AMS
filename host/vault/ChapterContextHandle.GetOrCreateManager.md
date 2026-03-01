---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/utility
  - llm/data-access
  - llm/error-handling
---
# ChapterContextHandle::GetOrCreateManager
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs`

## Summary
**Returns an existing `BookManager` for a book root or creates and caches one atomically.**

`GetOrCreateManager` provides thread-safe singleton-like manager reuse per book root using a static dictionary keyed by `ManagerKey(descriptor.RootPath)`. Inside a `lock (Sync)`, it attempts `Managers.TryGetValue`; on miss it creates `new BookManager(new[] { descriptor })`, stores it, and returns it. This method centralizes manager caching and prevents duplicate manager instances for the same root path under concurrent calls.


#### [[ChapterContextHandle.GetOrCreateManager]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static BookManager GetOrCreateManager(BookDescriptor descriptor)
```

**Called-by <-**
- [[ChapterContextHandle.Create]]

