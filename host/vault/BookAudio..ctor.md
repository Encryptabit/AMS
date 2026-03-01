---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookAudio.cs"
access_modifier: "internal"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/validation
  - llm/error-handling
---
# BookAudio::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookAudio.cs`

## Summary
**Initializes a `BookAudio` instance with the owning book context required for roomtone path resolution and lazy loading.**

The `BookAudio` constructor enforces a required `BookContext` dependency and stores it for later artifact resolution. It performs a null guard (`book ?? throw new ArgumentNullException(nameof(book))`) and otherwise has no side effects or eager loading. Lazy audio state (`_roomtone`, `_roomtoneLoaded`) remains at field defaults until accessed.


#### [[BookAudio..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal BookAudio(BookContext book)
```

