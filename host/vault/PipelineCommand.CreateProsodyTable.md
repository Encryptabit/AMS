---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
---
# PipelineCommand::CreateProsodyTable
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Construct a chapter prosody report table from `PauseStatsSet` data, returning no table when there are no detected pause intervals.**

`CreateProsodyTable` builds a `Spectre.Console` `Table` with fixed prosody columns (`Class`, `Count`, `Min/Median/Max/Mean/Total (s)`) and populates it by iterating `EnumerateStats(stats)` over each `PauseClass` bucket. It skips buckets where `pauseStats.Count == 0`, formats numeric values using invariant culture (`Count` as integer, durations as `"F3"`), and tracks a `hasRows` flag to return `null` when no pause classes contain data instead of emitting an empty table.


#### [[PipelineCommand.CreateProsodyTable]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Table CreateProsodyTable(PauseStatsSet stats)
```

**Calls ->**
- [[PipelineCommand.EnumerateStats]]

**Called-by <-**
- [[PipelineCommand.PrintStatsReport]]

