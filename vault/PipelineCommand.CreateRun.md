---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 14
tags:
  - method
---
# PipelineCommand::CreateRun
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


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

