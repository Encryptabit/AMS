---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# PipelineCommand::EnumerateStats
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Produce an ordered sequence of pause-class/stat pairs from a `PauseStatsSet` for downstream rendering.**

`EnumerateStats` is a private static iterator block that projects a `PauseStatsSet` into `IEnumerable<(PauseClass Class, PauseStats Stats)>` via seven explicit `yield return` statements. It binds each enum bucket to the matching property (`stats.Comma`, `stats.Sentence`, `stats.Paragraph`, `stats.ChapterHead`, `stats.PostChapterRead`, `stats.Tail`, `stats.Other`) in a deterministic order used by `CreateProsodyTable`. The method contains no branching, validation, or transformation logic beyond tuple construction.


#### [[PipelineCommand.EnumerateStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IEnumerable<(PauseClass Class, PauseStats Stats)> EnumerateStats(PauseStatsSet stats)
```

**Called-by <-**
- [[PipelineCommand.CreateProsodyTable]]

