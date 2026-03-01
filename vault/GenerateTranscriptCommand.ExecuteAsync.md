---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs"
access_modifier: "public"
complexity: 5
fan_in: 2
fan_out: 4
tags:
  - method
---
# GenerateTranscriptCommand::ExecuteAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs`


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

