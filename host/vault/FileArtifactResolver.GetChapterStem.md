---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/data-access
---
# FileArtifactResolver::GetChapterStem
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Computes the chapter artifact stem from `ChapterId`, or from the normalized chapter root folder name as a fallback.**

`GetChapterStem` derives the filename stem used for chapter artifacts. It returns `descriptor.ChapterId` when present (`!string.IsNullOrWhiteSpace`), otherwise it falls back to the chapter root directory name by calling `GetChapterRoot(descriptor)`, trimming trailing separators, and taking `Path.GetFileName(root)`. This gives a stable stem even when `ChapterId` is missing, while inheriting root-path validation from `GetChapterRoot`.


#### [[FileArtifactResolver.GetChapterStem]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string GetChapterStem(ChapterDescriptor descriptor)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterRoot]]

**Called-by <-**
- [[FileArtifactResolver.GetChapterArtifactPath]]
- [[FileArtifactResolver.GetTextGridPath]]

