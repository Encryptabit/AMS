---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "public"
complexity: 41
fan_in: 1
fan_out: 38
tags:
  - method
  - danger/high-complexity
---
# PipelineService::RunChapterAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/PipelineService.cs`

> [!danger] High Complexity (41)
> Cyclomatic complexity: 41. Consider refactoring into smaller methods.


#### [[PipelineService.RunChapterAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<PipelineChapterResult> RunChapterAsync(IWorkspace workspace, PipelineRunOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[BuildTranscriptIndexCommand.ExecuteAsync]]
- [[ComputeAnchorsCommand.ExecuteAsync]]
- [[GenerateTranscriptCommand.ExecuteAsync]]
- [[HydrateTranscriptCommand.ExecuteAsync]]
- [[MergeTimingsCommand.ExecuteAsync]]
- [[RunMfaCommand.ExecuteAsync]]
- [[MfaInvocationContext.BeginScope]]
- [[PipelineConcurrencyControl.RentMfaWorkspace]]
- [[PipelineConcurrencyControl.ReturnMfaWorkspace]]
- [[MfaProcessSupervisor.Shutdown]]
- [[ChapterContext.ResolveArtifactFile]]
- [[ChapterContextHandle.Save]]
- [[ChapterDocuments.GetAnchorsFile]]
- [[ChapterDocuments.GetAsrFile]]
- [[ChapterDocuments.GetHydratedTranscriptFile]]
- [[ChapterDocuments.GetTextGridFile]]
- [[ChapterDocuments.GetTranscriptFile]]
- [[IWorkspace.OpenChapter]]
- [[PipelineService.BuildDefaultAnchorOptions]]
- [[PipelineService.CopyTreatedAudio]]
- [[PipelineService.EnsureBookIndexAsync]]
- [[Ams.Core.Services.PipelineService.EnvironmentVariableScope.Dispose]]
- [[HasAnchorDocument]]
- [[HasAsrDocument]]
- [[HasHydrateDocument]]
- [[HasTextGridDocument]]
- [[HasTranscriptDocument]]
- [[PipelineService.IsStageEnabled]]
- [[PipelineService.Release]]
- [[ResolveAnchorsFile]]
- [[ResolveAsrFile]]
- [[ResolveHydrateFile]]
- [[PipelineService.ResolveTextGridFile]]
- [[ResolveTranscriptFile]]
- [[ResolveTreatedFile]]
- [[TextGridFile]]
- [[PipelineService.ValidateOptions]]
- [[PipelineService.WaitAsync]]

**Called-by <-**
- [[PipelineCommand.RunPipelineAsync]]

