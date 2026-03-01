---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Commands/ComputeAnchorsCommand.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 2
tags:
  - method
---
# ComputeAnchorsCommand::ExecuteAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Commands/ComputeAnchorsCommand.cs`


#### [[ComputeAnchorsCommand.ExecuteAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task ExecuteAsync(ChapterContext chapter, AnchorComputationOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[ChapterContext.Save]]
- [[IAlignmentService.ComputeAnchorsAsync]]

**Called-by <-**
- [[AlignCommand.CreateAnchors]]
- [[PipelineService.RunChapterAsync]]

