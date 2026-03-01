---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
---
# PipelineCommand::FormatDuration
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Formats a duration in seconds into an invariant-culture timestamp string for CLI report output.**

`FormatDuration` converts a raw `double` seconds value to a `TimeSpan` via `TimeSpan.FromSeconds(seconds)` and formats it using `ts.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture)`. The method is a deterministic formatter used by reporting paths (`CreateAudioTable`, `PrintStatsReport`) to render durations in fixed-width `HH:MM:SS.mmm` form without locale-specific variance.


#### [[PipelineCommand.FormatDuration]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatDuration(double seconds)
```

**Called-by <-**
- [[PipelineCommand.CreateAudioTable]]
- [[PipelineCommand.PrintStatsReport]]

