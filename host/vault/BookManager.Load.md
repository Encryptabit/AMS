---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
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
**Loads a book context at a specific descriptor index while updating cursor state and using cached context creation.**

`Load(int index)` resolves a `BookContext` by positional index with bounds validation and cache-backed retrieval. It uses an unsigned range check (`(uint)index >= (uint)_descriptors.Count`) to reject invalid indices via `ArgumentOutOfRangeException`, updates navigation state (`_cursor = index`), and returns `GetOrCreate(_descriptors[index])`. This delegates context reuse/creation to the manager’s internal cache path.


#### [[BookManager.Load]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookContext Load(int index)
```

**Calls ->**
- [[BookManager.GetOrCreate]]

**Called-by <-**
- [[BookManager.TryMoveNext]]
- [[BookManager.TryMovePrevious]]

