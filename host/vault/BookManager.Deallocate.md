---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/data-access
---
# BookManager::Deallocate
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`

## Summary
**Removes a cached book context by ID after saving it and deallocating its chapter resources.**

`Deallocate` performs an idempotent cache eviction for a book context keyed by `bookId`. It short-circuits on null/whitespace IDs, then attempts `_cache.Remove(bookId, out var context)`; when successful, it persists pending changes via `context.Save()`, releases chapter-level resources with `context.Chapters.DeallocateAll()`, and emits a debug log. Missing IDs are treated as a no-op.


#### [[BookManager.Deallocate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Deallocate(string bookId)
```

**Calls ->**
- [[Log.Debug]]
- [[BookContext.Save]]
- [[ChapterManager.DeallocateAll]]

