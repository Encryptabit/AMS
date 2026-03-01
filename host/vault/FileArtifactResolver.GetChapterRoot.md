---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "private"
complexity: 2
fan_in: 6
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
  - llm/data-access
---
# FileArtifactResolver::GetChapterRoot
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Ensures a chapter has a valid root path and returns it as an absolute filesystem path.**

`GetChapterRoot` validates `descriptor.RootPath` and throws `InvalidOperationException` when it is null/empty/whitespace, including the `ChapterId` in the error message for diagnostics. It then normalizes the configured root via `Path.GetFullPath(descriptor.RootPath)` and returns the absolute path. This method centralizes chapter-root validation and canonicalization for downstream path builders.


#### [[FileArtifactResolver.GetChapterRoot]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string GetChapterRoot(ChapterDescriptor descriptor)
```

**Called-by <-**
- [[FileArtifactResolver.GetChapterArtifactPath]]
- [[FileArtifactResolver.GetChapterStem]]
- [[FileArtifactResolver.GetPausePolicyFile]]
- [[FileArtifactResolver.GetTextGridPath]]
- [[FileArtifactResolver.LoadPausePolicy]]
- [[FileArtifactResolver.SavePausePolicy]]

