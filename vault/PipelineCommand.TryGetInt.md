---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
---
# PipelineCommand::TryGetInt
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.TryGetInt]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryGetInt(JsonElement element, string propertyName, out int value)
```

**Called-by <-**
- [[PipelineCommand.LoadSentenceTimings]]

