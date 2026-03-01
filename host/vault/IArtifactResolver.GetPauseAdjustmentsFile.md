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
# IArtifactResolver::GetPauseAdjustmentsFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Return the resolved `FileInfo` for a chapter’s `pause-adjustments.json` artifact.**

`GetPauseAdjustmentsFile` is declared on `IArtifactResolver` and implemented in `FileArtifactResolver` as `new(GetChapterArtifactPath(context, "pause-adjustments.json"))`. The method is side-effect free and only computes the canonical chapter artifact path, returning it as `FileInfo` for downstream document-slot metadata and persistence plumbing initialized in constructors.


#### [[IArtifactResolver.GetPauseAdjustmentsFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
FileInfo GetPauseAdjustmentsFile(ChapterContext context)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]

