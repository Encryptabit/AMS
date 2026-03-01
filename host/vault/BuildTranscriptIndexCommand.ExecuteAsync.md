---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/BuildTranscriptIndexCommand.cs"
access_modifier: "public"
complexity: 8
fan_in: 2
fan_out: 5
tags:
  - method
---
# BuildTranscriptIndexCommand::ExecuteAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/BuildTranscriptIndexCommand.cs`

## Summary
****

`ExecuteAsync(...)` is the chapter-level orchestration entrypoint used by `CreateTranscriptIndex` and `RunChapterAsync` to produce a transcript index from `ChapterContext` and `BuildTranscriptIndexOptions`. It resolves required inputs (`GetAsrFile`, `GetBookIndexFile`, `ResolveAudioFile`), then delegates the core build step to `BuildTranscriptIndexAsync` with the resolved artifacts and passed `CancellationToken`. After generation, it persists the result via `Save`, so the method primarily coordinates I/O resolution, build execution, and write-back.


#### [[BuildTranscriptIndexCommand.ExecuteAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task ExecuteAsync(ChapterContext chapter, BuildTranscriptIndexOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BuildTranscriptIndexCommand.ResolveAudioFile]]
- [[BookDocuments.GetBookIndexFile]]
- [[ChapterContext.Save]]
- [[ChapterDocuments.GetAsrFile]]
- [[IAlignmentService.BuildTranscriptIndexAsync]]

**Called-by <-**
- [[AlignCommand.CreateTranscriptIndex]]
- [[PipelineService.RunChapterAsync]]

