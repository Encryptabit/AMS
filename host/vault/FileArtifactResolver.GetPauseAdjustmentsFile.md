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
# FileArtifactResolver::GetPauseAdjustmentsFile
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Returns a `FileInfo` pointing to the chapter pause-adjustments JSON artifact location.**

`GetPauseAdjustmentsFile` is a thin path helper that returns a new `FileInfo` for the chapter pause-adjustments artifact. It delegates path resolution to `GetChapterArtifactPath(context, "pause-adjustments.json")` and does not perform existence checks or file IO. The method simply exposes the canonical artifact path as a typed filesystem object.


#### [[FileArtifactResolver.GetPauseAdjustmentsFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo GetPauseAdjustmentsFile(ChapterContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]

