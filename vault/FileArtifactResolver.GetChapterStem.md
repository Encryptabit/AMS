---
namespace: "Ams.Core.Runtime.Artifacts"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
---
# FileArtifactResolver::GetChapterStem
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Artifacts/FileArtifactResolver.cs`


#### [[FileArtifactResolver.GetChapterStem]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string GetChapterStem(ChapterDescriptor descriptor)
```

**Calls ->**
- [[FileArtifactResolver.GetChapterRoot]]

**Called-by <-**
- [[FileArtifactResolver.GetChapterArtifactPath]]
- [[FileArtifactResolver.GetTextGridPath]]

