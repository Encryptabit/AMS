---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs"
access_modifier: "internal"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
---
# ChapterDocuments::GetAnchorsFile
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs`


#### [[ChapterDocuments.GetAnchorsFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal FileInfo GetAnchorsFile()
```

**Calls ->**
- [[DocumentSlot_T_.GetBackingFile]]

**Called-by <-**
- [[AlignCommand.CreateAnchors]]
- [[PipelineService.RunChapterAsync]]

