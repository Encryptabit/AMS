---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 7
tags:
  - method
  - llm/async
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# PipelineCommand::RunPipelineForMultipleChaptersAsync
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Runs the chapter pipeline across multiple WAV files in parallel with controlled worker/ASR/MFA concurrency, progress reporting, and aggregated error propagation.**

`RunPipelineForMultipleChaptersAsync` is a private async orchestrator that validates inputs, filters `chapterFiles` to existing WAVs, and normalizes concurrency settings before launching work. It uses `PipelineConcurrencyControl.CreateShared(maxAsrParallelism, maxMfaParallelism)` plus a `SemaphoreSlim(maxWorkers)` to bound throughput, then schedules one task per chapter that updates reporter state (`SetQueued`, `MarkRunning`, `MarkComplete`/`MarkFailed`) around `RunPipelineAsync(...)`. Chapter-level failures and cancellations are wrapped into `InvalidOperationException`s and stored in a `ConcurrentBag`; after `Task.WhenAll`, the method throws an `AggregateException` if any chapter failed.


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

