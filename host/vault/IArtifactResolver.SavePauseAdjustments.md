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
  - llm/validation
  - llm/di
---
# IArtifactResolver::SavePauseAdjustments
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/IArtifactResolver.cs`

## Summary
**Persist chapter pause-adjustment data to the resolver’s `pause-adjustments.json` artifact file.**

`SavePauseAdjustments` is defined on `IArtifactResolver` and implemented in `FileArtifactResolver` as chapter artifact persistence to `pause-adjustments.json`. It validates input via `ArgumentNullException.ThrowIfNull(document)`, resolves the path with `GetChapterArtifactPath(context, "pause-adjustments.json")`, ensures the directory exists through `EnsureDirectory(path)`, then writes using `document.Save(path)`. `ChapterDocuments` registers this method in its constructor as the save delegate for the pause-adjustments `DocumentSlot`.


#### [[IArtifactResolver.SavePauseAdjustments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void SavePauseAdjustments(ChapterContext context, PauseAdjustmentsDocument document)
```

**Called-by <-**
- [[ChapterDocuments..ctor]]

