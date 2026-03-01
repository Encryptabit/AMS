---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# PipelineCommand::PrintStatsReport
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Render a human-readable pipeline statistics report for book-level and chapter-level audio/prosody analysis in the CLI.**

`PrintStatsReport` builds a terminal-facing statistics report with Spectre.Console (`AnsiConsole` + `Table`) for the pipeline root and analyzed chapters. It conditionally prints a book-level metrics table from `bookIndex.Totals` (words, sentences, paragraphs, estimated duration, analyzed audio duration) and computes an `Audio - Estimate` delta row when estimated duration is positive; if index data is unavailable, it emits warning markup instead. It then prints chapter count, iterates chapters sorted by name with `StringComparer.OrdinalIgnoreCase`, renders per-chapter audio stats via `CreateAudioTable`, and renders prosody via `CreateProsodyTable` with explicit warnings when prosody data or pause intervals are missing.


#### [[PipelineCommand.PrintStatsReport]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void PrintStatsReport(DirectoryInfo root, FileInfo bookIndexFile, BookIndex bookIndex, IReadOnlyList<PipelineCommand.ChapterStats> chapters, double totalAudioSec)
```

**Calls ->**
- [[PipelineCommand.CreateAudioTable]]
- [[PipelineCommand.CreateProsodyTable]]
- [[PipelineCommand.FormatDuration]]

**Called-by <-**
- [[PipelineCommand.RunStats]]

