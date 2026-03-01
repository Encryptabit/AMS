---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 3
tags:
  - method
---
# CompactPipelineProgressReporter::RunAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[CompactPipelineProgressReporter.RunAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task RunAsync(IReadOnlyList<FileInfo> chapters, Func<PipelineCommand.IPipelineProgressReporter, Task> run)
```

**Calls ->**
- [[CompactPipelineProgressReporter.Attach]]
- [[CompactPipelineProgressReporter.BuildView]]
- [[CompactPipelineProgressReporter.MarkFinished]]

**Called-by <-**
- [[PipelineCommand.CreateRun]]

