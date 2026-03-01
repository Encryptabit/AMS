---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "private"
complexity: 1
fan_in: 19
fan_out: 2
tags:
  - method
  - danger/high-fan-in
  - llm/utility
  - llm/data-access
---
# FileArtifactResolver::GetChapterArtifactPath
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

> [!danger] High Fan-In (19)
> This method is called by 19 other methods. Changes here have wide impact.

## Summary
**Constructs a standardized chapter artifact file path from chapter metadata and a suffix.**

`GetChapterArtifactPath` builds the canonical file path for chapter-scoped artifacts by combining the chapter directory and a stem-qualified suffix. It resolves `directory` via `GetChapterRoot(context.Descriptor)`, resolves `stem` via `GetChapterStem(context.Descriptor)`, then returns `Path.Combine(directory, $"{stem}.{suffix}")`. This centralizes naming conventions used by all chapter artifact load/save/file-handle methods.


#### [[FileArtifactResolver.GetChapterArtifactPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string GetChapterArtifactPath(ChapterContext context, string suffix)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterRoot]]
- [[FileArtifactResolver.GetChapterStem]]

**Called-by <-**
- [[FileArtifactResolver.GetAnchorsFile]]
- [[FileArtifactResolver.GetAsrFile]]
- [[FileArtifactResolver.GetAsrTranscriptTextFile]]
- [[FileArtifactResolver.GetChapterArtifactFile]]
- [[FileArtifactResolver.GetHydratedTranscriptFile]]
- [[FileArtifactResolver.GetPauseAdjustmentsFile]]
- [[FileArtifactResolver.GetTranscriptFile]]
- [[FileArtifactResolver.LoadAnchors]]
- [[FileArtifactResolver.LoadAsr]]
- [[FileArtifactResolver.LoadAsrTranscriptText]]
- [[FileArtifactResolver.LoadHydratedTranscript]]
- [[FileArtifactResolver.LoadPauseAdjustments]]
- [[FileArtifactResolver.LoadTranscript]]
- [[FileArtifactResolver.SaveAnchors]]
- [[FileArtifactResolver.SaveAsr]]
- [[FileArtifactResolver.SaveAsrTranscriptText]]
- [[FileArtifactResolver.SaveHydratedTranscript]]
- [[FileArtifactResolver.SavePauseAdjustments]]
- [[FileArtifactResolver.SaveTranscript]]

