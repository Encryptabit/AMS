---
namespace: "Ams.Core.Runtime.Workspace"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Workspace/IWorkspace.cs"
access_modifier: "public"
complexity: 1
fan_in: 8
fan_out: 0
tags:
  - method
---
# IWorkspace::OpenChapter
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Workspace/IWorkspace.cs`


#### [[IWorkspace.OpenChapter]]
##### What it does:
<member name="M:Ams.Core.Runtime.Workspace.IWorkspace.OpenChapter(Ams.Core.Runtime.Workspace.ChapterOpenOptions)">
    <summary>
    Opens (or creates) a chapter context according to the supplied options.
    Workspaces are responsible for filling in any missing defaults (e.g.,
    book-index path, chapter directory) that are specific to the host.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
ChapterContextHandle OpenChapter(ChapterOpenOptions options)
```

**Called-by <-**
- [[AlignCommand.CreateAnchors]]
- [[AlignCommand.CreateHydrateTx]]
- [[AlignCommand.CreateTranscriptIndex]]
- [[AsrCommand.Create]]
- [[TreatCommand.Create]]
- [[ValidateCommand.CreateReportCommand]]
- [[ValidateTimingSession.LoadSessionContextAsync]]
- [[PipelineService.RunChapterAsync]]

