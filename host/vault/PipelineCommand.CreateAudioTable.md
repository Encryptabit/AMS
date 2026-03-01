---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/factory
---
# PipelineCommand::CreateAudioTable
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Build a console-ready table of key audio metrics from an `AudioStats` object.**

`CreateAudioTable` constructs a new two-column `Table` (`"Metric"`, `"Value"`) and fills it with six fixed audio rows: length, sample peak, true peak, overall RMS, and max/min 0.5s window RMS. It delegates value formatting to `FormatDuration(audio.LengthSec)` for time and `FormatDb(...)` for level-based metrics, keeping display formatting centralized. The method is straight-line/deterministic and returns the populated table for use by `PrintStatsReport`.


#### [[PipelineCommand.CreateAudioTable]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Table CreateAudioTable(PipelineCommand.AudioStats audio)
```

**Calls ->**
- [[PipelineCommand.FormatDb]]
- [[PipelineCommand.FormatDuration]]

**Called-by <-**
- [[PipelineCommand.PrintStatsReport]]

