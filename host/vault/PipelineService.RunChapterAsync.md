---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/PipelineService.cs"
access_modifier: "public"
complexity: 41
fan_in: 1
fan_out: 38
tags:
  - method
  - danger/high-complexity
  - llm/entry-point
  - llm/async
  - llm/validation
  - llm/error-handling
  - llm/di
---
# PipelineService::RunChapterAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/PipelineService.cs`

> [!danger] High Complexity (41)
> Cyclomatic complexity: 41. Consider refactoring into smaller methods.

## Summary
**Runs a single chapter through the configured pipeline stages, coordinating dependencies/concurrency and returning the resulting artifact metadata and stage execution outcomes.**

`RunChapterAsync` is an async orchestration method that validates `PipelineRunOptions`, starts an MFA invocation scope, ensures the book index is present, opens the chapter from `IWorkspace`, and derives artifact state/path resolvers (`Has*Document`, `Resolve*File`) from chapter documents. It gates ASR, anchors, transcript indexing, hydration, and MFA execution using `IsStageEnabled`, `Force`, and existing artifact checks, invoking injected commands (`_generateTranscript`, `_computeAnchors`, `_buildTranscriptIndex`, `_hydrateTranscript`, `_runMfa`, `_mergeTimings`) with stage-specific option shaping and semaphore-based concurrency controls. For MFA, it can rent a dedicated workspace, temporarily bind `MFA_ROOT_DIR` via `EnvironmentVariableScope`, and shuts down the shared supervisor when rebinding is required; it then optionally merges TextGrid timings, copies treated audio, persists via `handle.Save()`, and returns a `PipelineChapterResult` with execution flags and resolved artifact files, throwing `InvalidOperationException` when expected artifact paths are unavailable.


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

