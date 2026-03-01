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
# IArtifactResolver::GetAnchorsFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Return a `FileInfo` for the chapter anchor artifact file (`align.anchors.json`) resolved from context.**

`GetAnchorsFile` is declared on `IArtifactResolver` and implemented in `FileArtifactResolver` as an expression-bodied path mapping: `new(GetChapterArtifactPath(context, "align.anchors.json"))`. It has no serialization or file access side effects; it only exposes the canonical anchor-artifact file handle used by constructor-wired document slots.


#### [[IArtifactResolver.GetAnchorsFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
FileInfo GetAnchorsFile(ChapterContext context)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]

