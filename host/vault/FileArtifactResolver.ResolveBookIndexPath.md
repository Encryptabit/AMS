---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "private"
complexity: 1
fan_in: 3
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
---
# FileArtifactResolver::ResolveBookIndexPath
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Builds the standard filesystem path for a book’s `book-index.json` artifact.**

`ResolveBookIndexPath` computes the canonical book-index artifact path by combining the normalized book root with the fixed filename `"book-index.json"`. It delegates root resolution/validation to `GetBookRoot(context)` and then uses `Path.Combine` for platform-correct path construction. The method performs no IO itself and serves as the shared path source for load/save/file-handle APIs.


#### [[FileArtifactResolver.ResolveBookIndexPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveBookIndexPath(BookContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetBookRoot]]

**Called-by <-**
- [[FileArtifactResolver.GetBookIndexFile]]
- [[FileArtifactResolver.LoadBookIndex]]
- [[FileArtifactResolver.SaveBookIndex]]

