---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 4
tags:
  - method
---
# FileArtifactResolver::LoadPausePolicy
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`


#### [[FileArtifactResolver.LoadPausePolicy]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PausePolicy LoadPausePolicy(ChapterContext context)
```

**Calls ->**
- [[PausePolicyPresets.House]]
- [[PausePolicyStorage.Load]]
- [[FileArtifactResolver.GetBookRoot]]
- [[FileArtifactResolver.GetChapterRoot]]

