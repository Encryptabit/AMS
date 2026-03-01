---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 9
fan_in: 2
fan_out: 2
tags:
  - method
---
# CompactPipelineProgressReporter::BuildView
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[CompactPipelineProgressReporter.BuildView]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Table BuildView()
```

**Calls ->**
- [[CompactPipelineProgressReporter.BuildStageMarkup]]
- [[CompactPipelineProgressReporter.FormatElapsed]]

**Called-by <-**
- [[CompactPipelineProgressReporter.RefreshUnsafe]]
- [[CompactPipelineProgressReporter.RunAsync]]

