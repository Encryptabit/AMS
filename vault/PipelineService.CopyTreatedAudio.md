---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
---
# PipelineService::CopyTreatedAudio
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs`


#### [[PipelineService.CopyTreatedAudio]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void CopyTreatedAudio(FileInfo source, FileInfo destination, bool overwrite)
```

**Called-by <-**
- [[PipelineService.RunChapterAsync]]

