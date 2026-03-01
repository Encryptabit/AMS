---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs"
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
# ChapterContext::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs`

## Summary
**Initializes a chapter runtime context by validating inputs and wiring document/audio managers against the parent book resolver.**

The `ChapterContext` constructor composes chapter runtime state from a parent `BookContext` and `ChapterDescriptor`. It null-checks both inputs (`ArgumentNullException`), captures the resolver from the parent book (`_resolver = book.Resolver`), and eagerly initializes subsystem wrappers: `Documents = new ChapterDocuments(this, _resolver)` and `Audio = new AudioBufferManager(descriptor.AudioBuffers)`. This creates a fully wired per-chapter context with document and audio services ready.


#### [[ChapterContext..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal ChapterContext(BookContext book, ChapterDescriptor descriptor)
```

