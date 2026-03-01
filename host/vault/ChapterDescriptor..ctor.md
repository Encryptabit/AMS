---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "public"
complexity: 5
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ChapterDescriptor::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`

## Summary
**Creates a chapter descriptor with required IDs/paths and optional audio/document/alias/word-range metadata.**

This constructor initializes an immutable `ChapterDescriptor` with required identity/path and optional metadata/range fields. It null-checks `chapterId` and `rootPath` (`ArgumentNullException`), assigns `AudioBuffers` with a null fallback to `Array.Empty<AudioBufferDescriptor>()`, keeps `Documents` as-is (nullable), and defaults `Aliases` to `Array.Empty<string>()` when null. Optional `bookStartWord`/`bookEndWord` are stored without additional validation.


#### [[ChapterDescriptor..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ChapterDescriptor(string chapterId, string rootPath, IReadOnlyList<AudioBufferDescriptor> audioBuffers, IReadOnlyDictionary<string, string> documents = null, IReadOnlyCollection<string> aliases = null, int? bookStartWord = null, int? bookEndWord = null)
```

