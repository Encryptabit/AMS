---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/entry-point
  - llm/factory
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# ChapterContextHandle::Create
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs`

## Summary
**Creates a chapter context handle from artifact file inputs by resolving a book manager and delegating chapter context creation.**

`Create` is a static factory that bootstraps a `ChapterContextHandle` from filesystem artifacts, validating that `bookIndexFile` is non-null and exists (throwing `FileNotFoundException` otherwise). It derives `bookRoot`, synthesizes a minimal `BookDescriptor` (book ID from root folder name, empty chapter list), resolves/reuses a `BookManager` via `GetOrCreateManager`, then delegates to `book.Chapters.CreateContext(...)` with optional artifact overrides. This centralizes ad-hoc context construction from files while reusing manager caching semantics.


#### [[ChapterContextHandle.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static ChapterContextHandle Create(FileInfo bookIndexFile, FileInfo asrFile = null, FileInfo transcriptFile = null, FileInfo hydrateFile = null, FileInfo audioFile = null, DirectoryInfo chapterDirectory = null, string chapterId = null)
```

**Calls ->**
- [[ChapterContextHandle.GetOrCreateManager]]
- [[ChapterManager.CreateContext]]

