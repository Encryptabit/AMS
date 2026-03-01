---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 2
tags:
  - method
---
# PipelineCommand::LoadSentenceTimings
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


#### [[PipelineCommand.LoadSentenceTimings]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyDictionary<int, SentenceTiming> LoadSentenceTimings(string hydratePath)
```

**Calls ->**
- [[PipelineCommand.TryGetInt]]
- [[PipelineCommand.TryReadTiming]]

**Called-by <-**
- [[PipelineCommand.RunVerify]]

