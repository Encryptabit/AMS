---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
---
# CompactPipelineProgressReporter::RefreshUnsafe
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[CompactPipelineProgressReporter.RefreshUnsafe]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void RefreshUnsafe()
```

**Calls ->**
- [[CompactPipelineProgressReporter.BuildView]]

**Called-by <-**
- [[CompactPipelineProgressReporter.MarkFinished]]
- [[CompactPipelineProgressReporter.UpdateChapter]]

