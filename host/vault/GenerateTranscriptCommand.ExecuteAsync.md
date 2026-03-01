---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs"
access_modifier: "public"
complexity: 5
fan_in: 2
fan_out: 4
tags:
  - method
---
# GenerateTranscriptCommand::ExecuteAsync
**Path**: `Projects/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs`

## Summary
****

`ExecuteAsync(...)` is the chapter-level orchestration entrypoint used by `Create` and `RunChapterAsync` to produce transcripts from `GenerateTranscriptOptions`. It resolves the effective ASR execution context via `Resolve`, then runs the selected engine path (`RunNemoAsync` or `RunWhisperAsync`) to generate transcript output. It finishes by persisting results through `Save`, with the provided `CancellationToken` flowing through the async execution path.


#### [[GenerateTranscriptCommand.ExecuteAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task ExecuteAsync(ChapterContext chapter, GenerateTranscriptOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[GenerateTranscriptCommand.RunNemoAsync]]
- [[GenerateTranscriptCommand.RunWhisperAsync]]
- [[AsrEngineConfig.Resolve]]
- [[ChapterContext.Save]]

**Called-by <-**
- [[AsrCommand.Create]]
- [[PipelineService.RunChapterAsync]]

