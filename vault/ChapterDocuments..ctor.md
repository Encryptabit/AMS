---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs"
access_modifier: "internal"
complexity: 1
fan_in: 0
fan_out: 26
tags:
  - method
---
# ChapterDocuments::.ctor
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs`


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

