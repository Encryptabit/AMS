---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs"
access_modifier: "internal"
complexity: 1
fan_in: 3
fan_out: 1
tags:
  - method
---
# ChapterDocuments::GetHydratedTranscriptFile
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterDocuments.cs`


#### [[ChapterDocuments.GetHydratedTranscriptFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal FileInfo GetHydratedTranscriptFile()
```

**Calls ->**
- [[DocumentSlot_T_.GetBackingFile]]

**Called-by <-**
- [[AlignCommand.CreateHydrateTx]]
- [[RunMfaCommand.ExecuteAsync]]
- [[PipelineService.RunChapterAsync]]

