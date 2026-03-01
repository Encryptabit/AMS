---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Commands/BuildTranscriptIndexCommand.cs"
access_modifier: "public"
complexity: 8
fan_in: 2
fan_out: 5
tags:
  - method
---
# BuildTranscriptIndexCommand::ExecuteAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Commands/BuildTranscriptIndexCommand.cs`


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

