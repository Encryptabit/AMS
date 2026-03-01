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
# FileArtifactResolver::LoadTranscript
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

## Summary
**Loads the chapter transcript index artifact from its standard JSON path.**

`LoadTranscript` is a thin delegating wrapper that computes the transcript artifact path for a chapter (`GetChapterArtifactPath(context, "align.tx.json")`) and passes it to the generic loader (`LoadJson<TranscriptIndex>`). It contains no local validation, transformation, or fallback logic, so existence/deserialization behavior is fully defined by `LoadJson`. The method simply returns the loaded `TranscriptIndex` (or null if `LoadJson` is nullable-aware in this codebase).


#### [[FileArtifactResolver.LoadTranscript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public TranscriptIndex LoadTranscript(ChapterContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterArtifactPath]]
- [[FileArtifactResolver.LoadJson]]

