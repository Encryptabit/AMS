---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 29
fan_in: 2
fan_out: 7
tags:
  - method
  - danger/high-complexity
---
# PipelineCommand::RunPipelineAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

> [!danger] High Complexity (29)
> Cyclomatic complexity: 29. Consider refactoring into smaller methods.


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

