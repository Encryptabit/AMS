---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "private"
complexity: 1
fan_in: 19
fan_out: 2
tags:
  - method
  - danger/high-fan-in
---
# FileArtifactResolver::GetChapterArtifactPath
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`

> [!danger] High Fan-In (19)
> This method is called by 19 other methods. Changes here have wide impact.


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

