---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 2
tags:
  - method
---
# FileArtifactResolver::GetTextGridPath
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`


#### [[FileArtifactResolver.GetTextGridPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string GetTextGridPath(ChapterContext context)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterRoot]]
- [[FileArtifactResolver.GetChapterStem]]

**Called-by <-**
- [[FileArtifactResolver.GetTextGridFile]]
- [[FileArtifactResolver.LoadTextGrid]]

