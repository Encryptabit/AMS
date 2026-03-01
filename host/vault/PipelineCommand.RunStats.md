---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 14
fan_in: 1
fan_out: 7
tags:
  - method
  - llm/entry-point
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::RunStats
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Generate and print aggregated chapter statistics from the pipeline workspace, optionally using book-index metadata.**

`RunStats` resolves the working root with `CommandInputResolver.ResolveDirectory`, verifies it exists, then probes `book-index.json` (or `bookIndexOption`) and attempts `LoadJson<BookIndex>`, downgrading to `null` with debug logging on missing/invalid index data. It selects chapter folders via `ResolveChapterDirectories`, iterates with `cancellationToken.ThrowIfCancellationRequested()`, and invokes `ComputeChapterStats` per chapter, collecting non-null `ChapterStats` and summing `stats.Audio.LengthSec` while isolating per-chapter failures in `try/catch` + `Log.Debug`. It exits early on missing root, no chapters, or zero generated stats, and otherwise calls `PrintStatsReport(root, bookIndexFile, bookIndex, statsList, totalAudioSec)`.


#### [[PipelineCommand.RunStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void RunStats(DirectoryInfo workDirOption, FileInfo bookIndexOption, string chapterName, bool analyzeAll, CancellationToken cancellationToken)
```

**Calls ->**
- [[PipelineCommand.ComputeChapterStats]]
- [[PipelineCommand.LoadJson]]
- [[PipelineCommand.PrintStatsReport]]
- [[PipelineCommand.ResolveChapterDirectories]]
- [[CommandInputResolver.ResolveDirectory]]
- [[Log.Debug]]
- [[Log.Error_2]]

**Called-by <-**
- [[PipelineCommand.CreateStatsCommand]]

