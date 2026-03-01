---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs"
access_modifier: "public"
complexity: 3
fan_in: 2
fan_out: 1
tags:
  - method
---
# ChapterContext::ResolveArtifactFile
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterContext.cs`


#### [[ChapterContext.ResolveArtifactFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo ResolveArtifactFile(string suffix)
```

**Calls ->**
- [[IArtifactResolver.GetChapterArtifactFile]]

**Called-by <-**
- [[TreatCommand.Create]]
- [[PipelineService.RunChapterAsync]]

