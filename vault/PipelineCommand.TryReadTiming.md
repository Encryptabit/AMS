---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 1
tags:
  - method
---
# PipelineCommand::TryReadTiming
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.TryReadTiming]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryReadTiming(JsonElement sentence, out double start, out double end)
```

**Calls ->**
- [[PipelineCommand.TryGetDouble]]

**Called-by <-**
- [[PipelineCommand.LoadSentenceTimings]]

