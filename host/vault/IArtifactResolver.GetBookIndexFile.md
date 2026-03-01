---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/di
---
# IArtifactResolver::GetBookIndexFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Return a `FileInfo` pointing to the resolved `book-index.json` path for a given book context.**

`GetBookIndexFile` on `IArtifactResolver` is implemented in `FileArtifactResolver` as an expression-bodied wrapper that returns `new FileInfo(ResolveBookIndexPath(context))`. `ResolveBookIndexPath` composes `<book-root>/book-index.json` using `GetBookRoot(context)`, so the returned handle reflects the canonical persisted location and inherits root-path validation behavior from `GetBookRoot`.


#### [[IArtifactResolver.GetBookIndexFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
FileInfo GetBookIndexFile(BookContext context)
```

**Called-by <-**
- [[BookDocuments..ctor]]

