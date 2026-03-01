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
  - llm/di
  - llm/error-handling
---
# IArtifactResolver::LoadPauseAdjustments
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Load chapter-level pause-adjustment data from `pause-adjustments.json` with fail-soft behavior for missing or invalid files.**

`LoadPauseAdjustments` on `IArtifactResolver` is implemented in `FileArtifactResolver` as `LoadPauseAdjustmentsInternal(GetChapterArtifactPath(context, "pause-adjustments.json"))`. The internal loader returns `null` when the file does not exist, and it also wraps `PauseAdjustmentsDocument.Load(path)` in a broad `try/catch`, returning `null` on read/parse failures instead of throwing. `ChapterDocuments` wires this method in its constructor as the load delegate for the pause-adjustments document slot.


#### [[IArtifactResolver.LoadPauseAdjustments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
PauseAdjustmentsDocument LoadPauseAdjustments(ChapterContext context)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]

