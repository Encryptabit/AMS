---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/data-access
  - llm/utility
---
# FileArtifactResolver::LoadPauseAdjustments
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Loads a chapter’s pause-adjustments document from its standard artifact file via the internal adjustments loader.**

`LoadPauseAdjustments` is a thin wrapper that resolves the canonical adjustments artifact path (`GetChapterArtifactPath(context, "pause-adjustments.json")`) and delegates loading/parsing to `LoadPauseAdjustmentsInternal`. It contains no local validation, transformation, or fallback behavior. Result semantics, including null handling and parse errors, are defined by the internal loader.


#### [[FileArtifactResolver.LoadPauseAdjustments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PauseAdjustmentsDocument LoadPauseAdjustments(ChapterContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]
- [[FileArtifactResolver.LoadPauseAdjustmentsInternal]]

