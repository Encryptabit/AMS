---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 29
fan_in: 2
fan_out: 7
tags:
  - method
  - danger/high-complexity
  - llm/data-access
  - llm/async
  - llm/utility
  - llm/validation
  - llm/error-handling
  - llm/di
---
# PipelineCommand::RunPipelineAsync
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

> [!danger] High Complexity (29)
> Cyclomatic complexity: 29. Consider refactoring into smaller methods.

## Summary
**Runs the end-to-end chapter processing pipeline by preparing filesystem context and options, invoking the pipeline service asynchronously, and reporting/logging stage and output results.**

`RunPipelineAsync` is a private async orchestration method that validates required inputs (`pipelineService`, `bookFile`, `audioFile`), resolves/creates workspace and chapter directories, and normalizes the chapter stem via `MakeSafeFileStem` before building all artifact file paths. It constructs `GenerateTranscriptOptions`, shared `AnchorComputationOptions`, and a `PipelineRunOptions` aggregate (including transcript index, MFA, merge, treated-audio, and concurrency settings), then resolves the workspace from `bookIndexFile` and awaits `pipelineService.RunChapterAsync(...)`. After execution, it reports per-stage outcomes through `IPipelineProgressReporter.ReportStage` and logs cached vs executed status plus final output paths with `LogStageInfo`, including explicit handling of missing TextGrid (`"MFA missing"`).


#### [[PipelineCommand.RunPipelineAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task RunPipelineAsync(PipelineService pipelineService, FileInfo bookFile, FileInfo audioFile, DirectoryInfo workDirOption, FileInfo bookIndexOverride, string chapterIdOverride, bool forceIndex, bool force, double avgWpm, string asrServiceUrl, string asrModel, string asrLanguage, bool verbose, PipelineCommand.IPipelineProgressReporter progress, PipelineConcurrencyControl concurrency, CancellationToken cancellationToken)
```

**Calls ->**
- [[PipelineCommand.EnsureDirectory]]
- [[IPipelineProgressReporter.ReportStage]]
- [[PipelineCommand.LogStageInfo]]
- [[PipelineCommand.MakeSafeFileStem]]
- [[CommandInputResolver.ResolveWorkspace]]
- [[AsrEngineConfig.Resolve]]
- [[PipelineService.RunChapterAsync]]

**Called-by <-**
- [[PipelineCommand.CreateRun]]
- [[PipelineCommand.RunPipelineForMultipleChaptersAsync]]

