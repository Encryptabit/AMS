---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookContext.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# BookContext::ResolveArtifactFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookContext.cs`

## Summary
**Validates and resolves a book-level artifact filename to its `FileInfo` via the artifact resolver.**

`ResolveArtifactFile` validates the requested artifact filename and delegates path resolution to the configured artifact resolver. It throws `ArgumentException` when `fileName` is null/whitespace, trims the value, and returns `_resolver.GetBookArtifactFile(this, fileName.Trim())`. The method centralizes book-scoped artifact file lookup with input sanitization.


#### [[BookContext.ResolveArtifactFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo ResolveArtifactFile(string fileName)
```

**Calls ->**
- [[IArtifactResolver.GetBookArtifactFile]]

