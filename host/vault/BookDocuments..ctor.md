---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookDocuments.cs"
access_modifier: "internal"
complexity: 1
fan_in: 0
fan_out: 3
tags:
  - method
  - llm/di
  - llm/factory
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# BookDocuments::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookDocuments.cs`

## Summary
**Initializes book document storage by binding book-index load/save/file-access behavior to a managed document slot.**

The `BookDocuments` constructor wires a `DocumentSlot<BookIndex>` around resolver-backed load/save delegates for the owning `BookContext`. It validates both inputs with `ArgumentNullException.ThrowIfNull`, then configures `_bookIndex` with loader (`resolver.LoadBookIndex(context)`), saver (`resolver.SaveBookIndex(context, value)`), and `DocumentSlotOptions` exposing `BackingFileAccessor` via `resolver.GetBookIndexFile(context)`. This establishes lazy document retrieval, persistence, and backing-file introspection through a single slot abstraction.


#### [[BookDocuments..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal BookDocuments(BookContext context, IArtifactResolver resolver)
```

**Calls ->**
- [[IArtifactResolver.GetBookIndexFile]]
- [[IArtifactResolver.LoadBookIndex]]
- [[IArtifactResolver.SaveBookIndex]]

