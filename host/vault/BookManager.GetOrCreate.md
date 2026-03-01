---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/factory
  - llm/utility
  - llm/di
---
# BookManager::GetOrCreate
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`

## Summary
**Returns an existing cached book context or creates and caches one for the given descriptor.**

`GetOrCreate` implements cache-backed `BookContext` retrieval keyed by `descriptor.BookId`. It checks `_cache.TryGetValue`; on miss, it constructs `new BookContext(descriptor, _artifactResolver)`, stores it in `_cache`, and logs creation, while hits log reuse. The method always returns the resolved context instance, centralizing lazy context instantiation and reuse.


#### [[BookManager.GetOrCreate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private BookContext GetOrCreate(BookDescriptor descriptor)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[BookManager.Load]]
- [[BookManager.Load_2]]

