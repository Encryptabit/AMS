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
  - llm/error-handling
---
# IArtifactResolver::GetChapterArtifactFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Build and return a `FileInfo` for an arbitrary chapter-scoped artifact file using the resolver’s standard stem-plus-suffix path convention.**

`GetChapterArtifactFile` on `IArtifactResolver` is implemented in `FileArtifactResolver` as `new(GetChapterArtifactPath(context, suffix))`, returning a `FileInfo` without touching disk. `GetChapterArtifactPath` applies the chapter artifact naming convention by combining the chapter root with `<chapter-stem>.<suffix>`, where stem comes from `ChapterId` (or folder name fallback). Because root resolution flows through `GetChapterRoot`, invalid/missing chapter root metadata surfaces as `InvalidOperationException` during path resolution.


#### [[IArtifactResolver.GetChapterArtifactFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
FileInfo GetChapterArtifactFile(ChapterContext context, string suffix)
```

**Called-by <-**
- [[ChapterContext.ResolveArtifactFile]]

