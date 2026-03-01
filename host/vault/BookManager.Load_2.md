---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# BookManager::Load
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`

## Summary
**Loads a book context by book ID, updating cursor position and reusing/creating cached context state.**

`Load(string bookId)` resolves a book context by ID using a case-insensitive linear scan over `_descriptors`. It validates input with `ArgumentException.ThrowIfNullOrEmpty(bookId)`, updates `_cursor` to the matched index, and returns `GetOrCreate(_descriptors[i])` for cache-backed context reuse/creation. If no matching descriptor is found, it throws `KeyNotFoundException` with the missing ID.


#### [[BookManager.Load_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookContext Load(string bookId)
```

**Calls ->**
- [[BookManager.GetOrCreate]]

