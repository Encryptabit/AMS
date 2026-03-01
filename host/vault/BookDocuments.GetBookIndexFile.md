---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookDocuments.cs"
access_modifier: "internal"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
---
# BookDocuments::GetBookIndexFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookDocuments.cs`

## Summary
**Exposes the resolved backing file handle for the managed book-index document slot.**

`GetBookIndexFile` is an internal accessor that delegates to `_bookIndex.GetBackingFile()` and returns the current backing file reference for the book-index document slot. It does not compute paths directly or touch the filesystem; behavior depends on the slot’s configured `BackingFileAccessor`. The return type is nullable in implementation (`FileInfo?`), reflecting that a backing file may be unavailable.


#### [[BookDocuments.GetBookIndexFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal FileInfo GetBookIndexFile()
```

**Calls ->**
- [[DocumentSlot_T_.GetBackingFile]]

**Called-by <-**
- [[BuildTranscriptIndexCommand.ExecuteAsync]]

