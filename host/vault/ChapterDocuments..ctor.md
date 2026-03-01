---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs"
access_modifier: "internal"
complexity: 1
fan_in: 0
fan_out: 26
tags:
  - method
  - llm/di
  - llm/factory
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# ChapterDocuments::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs`

## Summary
**Initializes all chapter document slots by binding artifact-specific load/save/file-access behavior to the chapter resolver.**

The `ChapterDocuments` constructor composes resolver-backed `DocumentSlot<T>` instances for all chapter artifacts (transcript, hydrated transcript, anchors, ASR, ASR text, pause adjustments, pause policy, text grid). It validates `context`/`resolver`, defines a local `CreateOptions` helper for backing-file access, write-through, and post-load transforms, then wires each slot with load/save delegates from `IArtifactResolver`. Notable behaviors include write-through for ASR transcript text, defaulting null pause policy via `PausePolicyPresets.House()`, and a text-grid post-load transform that normalizes `SourcePath` to the resolver backing file. This constructor establishes the document orchestration layer without performing immediate IO.


#### [[ChapterDocuments..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal ChapterDocuments(ChapterContext context, IArtifactResolver resolver)
```

**Calls ->**
- [[PausePolicyPresets.House]]
- [[IArtifactResolver.GetAnchorsFile]]
- [[IArtifactResolver.GetAsrFile]]
- [[IArtifactResolver.GetAsrTranscriptTextFile]]
- [[IArtifactResolver.GetHydratedTranscriptFile]]
- [[IArtifactResolver.GetPauseAdjustmentsFile]]
- [[IArtifactResolver.GetPausePolicyFile]]
- [[IArtifactResolver.GetTextGridFile]]
- [[IArtifactResolver.GetTranscriptFile]]
- [[IArtifactResolver.LoadAnchors]]
- [[IArtifactResolver.LoadAsr]]
- [[IArtifactResolver.LoadAsrTranscriptText]]
- [[IArtifactResolver.LoadHydratedTranscript]]
- [[IArtifactResolver.LoadPauseAdjustments]]
- [[IArtifactResolver.LoadPausePolicy]]
- [[IArtifactResolver.LoadTextGrid]]
- [[IArtifactResolver.LoadTranscript]]
- [[IArtifactResolver.SaveAnchors]]
- [[IArtifactResolver.SaveAsr]]
- [[IArtifactResolver.SaveAsrTranscriptText]]
- [[IArtifactResolver.SaveHydratedTranscript]]
- [[IArtifactResolver.SavePauseAdjustments]]
- [[IArtifactResolver.SavePausePolicy]]
- [[IArtifactResolver.SaveTextGrid]]
- [[IArtifactResolver.SaveTranscript]]
- [[CreateOptions]]

