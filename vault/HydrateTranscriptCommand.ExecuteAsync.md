---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Commands/HydrateTranscriptCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 2
fan_out: 2
tags:
  - method
---
# HydrateTranscriptCommand::ExecuteAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Commands/HydrateTranscriptCommand.cs`


#### [[HydrateTranscriptCommand.ExecuteAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task ExecuteAsync(ChapterContext chapter, HydrationOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[ChapterContext.Save]]
- [[IAlignmentService.HydrateTranscriptAsync]]

**Called-by <-**
- [[AlignCommand.CreateHydrateTx]]
- [[PipelineService.RunChapterAsync]]

