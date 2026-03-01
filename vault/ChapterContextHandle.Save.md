---
namespace: "Ams.Core.Runtime.Chapter"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs"
access_modifier: "public"
complexity: 1
fan_in: 7
fan_out: 2
tags:
  - method
---
# ChapterContextHandle::Save
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Chapter/ChapterContextHandle.cs`


#### [[ChapterContextHandle.Save]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Save()
```

**Calls ->**
- [[BookContext.Save]]
- [[ChapterContext.Save]]

**Called-by <-**
- [[AlignCommand.CreateAnchors]]
- [[AlignCommand.CreateHydrateTx]]
- [[AlignCommand.CreateTranscriptIndex]]
- [[AsrCommand.Create]]
- [[ValidateCommand.CreateReportCommand]]
- [[ValidateTimingSession.LoadSessionContextAsync]]
- [[PipelineService.RunChapterAsync]]

