---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 14
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/factory
  - llm/di
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::CreateRun
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Creates and configures the CLI `run` command that orchestrates end-to-end pipeline execution for either one chapter or multiple chapters.**

`CreateRun` is a command-factory method that constructs the `run` subcommand, defines options for manuscript/audio/index paths, force flags, ASR settings, concurrency, and progress display, then attaches an async handler. The handler resolves/normalizes inputs (`ResolveBookSource`, `ResolveBookIndex`, `RequireAudio`), disables progress UI when debug logging is active, and branches between REPL multi-chapter execution (`CompactPipelineProgressReporter.RunAsync` + `RunPipelineForMultipleChaptersAsync`) and single-chapter execution (`RunPipelineAsync`) with `PipelineProgressReporter` lifecycle calls (`SetQueued`, `MarkRunning`, `MarkComplete`, `MarkFailed`) and `PipelineConcurrencyControl.CreateSingle`. Failures are trapped with cancellation/general exception handling, `Log.Error` logging, and non-zero `context.ExitCode` assignment.


#### [[PipelineCommand.CreateRun]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateRun(PipelineService pipelineService)
```

**Calls ->**
- [[CompactPipelineProgressReporter.RunAsync]]
- [[PipelineProgressReporter.MarkComplete]]
- [[PipelineProgressReporter.MarkFailed]]
- [[PipelineProgressReporter.MarkRunning]]
- [[PipelineProgressReporter.SetQueued]]
- [[PipelineCommand.RunPipelineAsync]]
- [[PipelineCommand.RunPipelineForMultipleChaptersAsync]]
- [[CommandInputResolver.RequireAudio]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[CommandInputResolver.ResolveBookSource]]
- [[PipelineConcurrencyControl.CreateSingle]]
- [[Log.Debug]]
- [[Log.Error]]
- [[Log.IsDebugLoggingEnabled]]

**Called-by <-**
- [[PipelineCommand.Create]]

