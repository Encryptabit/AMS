---
namespace: "Ams.Core.Application.Commands"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Commands/RunMfaCommand.cs"
access_modifier: "public"
complexity: 19
fan_in: 1
fan_out: 5
tags:
  - method
  - danger/high-complexity
---
# RunMfaCommand::ExecuteAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Commands/RunMfaCommand.cs`

> [!danger] High Complexity (19)
> Cyclomatic complexity: 19. Consider refactoring into smaller methods.


#### [[RunMfaCommand.ExecuteAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<RunMfaResult> ExecuteAsync(ChapterContext chapter, RunMfaOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[RunMfaCommand.ResolveAudioFile]]
- [[MfaWorkflow.RunChapterAsync]]
- [[ChapterDocuments.GetHydratedTranscriptFile]]
- [[ChapterDocuments.GetTextGridFile]]
- [[ChapterDocuments.InvalidateTextGrid]]

**Called-by <-**
- [[PipelineService.RunChapterAsync]]

