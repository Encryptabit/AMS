---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 3
tags:
  - method
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# FileArtifactResolver::SavePauseAdjustments
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Validates and persists the chapter pause-adjustments document to its canonical artifact path.**

`SavePauseAdjustments` writes a chapter’s pause-adjustments artifact to disk using the model’s own persistence API. It validates `document` (`ArgumentNullException.ThrowIfNull`), resolves `pause-adjustments.json` via `GetChapterArtifactPath`, ensures the destination directory exists with `EnsureDirectory`, then calls `document.Save(path)`. The method is synchronous and delegates serialization format details to `PauseAdjustmentsDocument.Save`.


#### [[FileArtifactResolver.SavePauseAdjustments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SavePauseAdjustments(ChapterContext context, PauseAdjustmentsDocument document)
```

**Calls ->**
- [[PauseAdjustmentsDocument.Save]]
- [[FileArtifactResolver.EnsureDirectory]]
- [[FileArtifactResolver.GetChapterArtifactPath]]

