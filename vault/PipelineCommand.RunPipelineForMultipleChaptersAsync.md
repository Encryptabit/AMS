---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 7
tags:
  - method
---
# PipelineCommand::RunPipelineForMultipleChaptersAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.RunPipelineForMultipleChaptersAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task RunPipelineForMultipleChaptersAsync(PipelineService pipelineService, FileInfo bookFile, DirectoryInfo workDirOption, FileInfo bookIndexOverride, bool forceIndex, bool force, double avgWpm, string asrServiceUrl, string asrModel, string asrLanguage, bool verbose, IReadOnlyList<FileInfo> chapterFiles, int maxWorkers, int maxAsrParallelism, int maxMfaParallelism, PipelineCommand.IPipelineProgressReporter reporter, CancellationToken cancellationToken)
```

**Calls ->**
- [[IPipelineProgressReporter.MarkComplete]]
- [[IPipelineProgressReporter.MarkFailed]]
- [[IPipelineProgressReporter.MarkRunning]]
- [[IPipelineProgressReporter.SetQueued]]
- [[PipelineCommand.RunPipelineAsync]]
- [[PipelineConcurrencyControl.CreateShared]]
- [[Log.Debug]]

**Called-by <-**
- [[PipelineCommand.CreateRun]]

