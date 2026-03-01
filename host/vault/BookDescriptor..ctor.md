---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# BookDescriptor::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`

## Summary
**Creates a book descriptor from required ID/root path plus optional chapter and document metadata.**

The constructor initializes an immutable `BookDescriptor` with required book identity/path and optional document metadata. It enforces non-null `bookId` and `rootPath` via `ArgumentNullException`, assigns `Chapters` with a null fallback to `Array.Empty<ChapterDescriptor>()`, and stores `Documents` as the provided nullable dictionary. No additional normalization or semantic validation is performed.


#### [[BookDescriptor..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookDescriptor(string bookId, string rootPath, IReadOnlyList<ChapterDescriptor> chapters, IReadOnlyDictionary<string, string> documents = null)
```

