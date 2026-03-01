---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookContext.cs"
access_modifier: "internal"
complexity: 3
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# BookContext::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookContext.cs`

## Summary
**Creates a fully initialized book runtime context by validating inputs and constructing document, chapter, and audio subsystems.**

The `BookContext` constructor wires core book runtime dependencies and child managers. It null-checks both `descriptor` and `resolver` (`ArgumentNullException`), stores them (`Descriptor`, `_resolver`), and eagerly instantiates `Documents` (`new BookDocuments(this, _resolver)`), `Chapters` (`new ChapterManager(this)`), and `Audio` (`new BookAudio(this)`). This establishes a fully composed context graph at construction time.


#### [[BookContext..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal BookContext(BookDescriptor descriptor, IArtifactResolver resolver)
```

