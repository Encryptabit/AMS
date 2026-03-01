---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 3
tags:
  - method
---
# FileArtifactResolver::SavePauseAdjustments
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`


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

