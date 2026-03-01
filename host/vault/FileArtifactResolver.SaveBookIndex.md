---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# FileArtifactResolver::SaveBookIndex
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Validates and writes the book index artifact to disk for the given book context.**

`SaveBookIndex` persists a `BookIndex` artifact to the resolver’s book-index location. It first enforces `bookIndex` non-null (`ArgumentNullException.ThrowIfNull`), computes the target path with `ResolveBookIndexPath(context)`, ensures the parent directory exists via `EnsureDirectory(path)`, then writes serialized JSON (`JsonSerializer.Serialize(..., JsonOptions)`) using `File.WriteAllText`. The method is synchronous and overwrites any existing file at that path.


#### [[FileArtifactResolver.SaveBookIndex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SaveBookIndex(BookContext context, BookIndex bookIndex)
```

**Calls ->**
- [[FileArtifactResolver.EnsureDirectory]]
- [[FileArtifactResolver.ResolveBookIndexPath]]

