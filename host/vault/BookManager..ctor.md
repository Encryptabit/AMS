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
  - llm/di
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# BookManager::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`

## Summary
**Initializes a book manager with validated book descriptors, resolver dependency, cache, and navigation state.**

The `BookManager` constructor validates and wires core runtime dependencies for book-context management. It requires a non-null, non-empty `descriptors` list (throwing `ArgumentNullException`/`ArgumentException` otherwise), initializes an ordinal-ignore-case cache dictionary, and sets `_artifactResolver` to the injected resolver or falls back to `FileArtifactResolver.Instance`. It also initializes cursor state (`_cursor = 0`) for index-based navigation.


#### [[BookManager..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookManager(IReadOnlyList<BookDescriptor> descriptors, IArtifactResolver resolver = null)
```

