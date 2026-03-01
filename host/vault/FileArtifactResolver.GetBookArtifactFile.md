---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/factory
---
# FileArtifactResolver::GetBookArtifactFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Builds a `FileInfo` reference for an artifact file located under the book root directory.**

`GetBookArtifactFile` is a one-line helper that returns a `FileInfo` for a book-scoped artifact path. It composes the path with `Path.Combine(GetBookRoot(context), fileName)` and wraps it in `FileInfo`, without touching the filesystem. The method performs no argument validation or existence checks.


#### [[FileArtifactResolver.GetBookArtifactFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo GetBookArtifactFile(BookContext context, string fileName)
```

**Calls ->**
- [[FileArtifactResolver.GetBookRoot]]

