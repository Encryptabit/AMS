---
namespace: "Ams.Core.Runtime.Workspace"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Workspace/IWorkspace.cs"
access_modifier: "public"
complexity: 1
fan_in: 8
fan_out: 0
tags:
  - method
  - llm/entry-point
  - llm/factory
  - llm/di
  - llm/validation
---
# IWorkspace::OpenChapter
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Workspace/IWorkspace.cs`

## Summary
**Declares the workspace API for opening or creating a chapter context handle from chapter-open options.**

`OpenChapter(ChapterOpenOptions options)` is an `IWorkspace` interface contract that defines chapter-context acquisition/creation from host-supplied open options. The XML docs specify workspace-owned defaulting responsibilities (for example, inferring book-index path and chapter directory when omitted). As an interface method, it provides no implementation and leaves option normalization, context reuse, and failure behavior to concrete workspace types.


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

