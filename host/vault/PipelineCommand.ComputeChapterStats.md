---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 7
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::ComputeChapterStats
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Build chapter-level audio and optional prosody statistics from discovered alignment/audio artifacts without hard-failing on missing or malformed enrichment inputs.**

`ComputeChapterStats` probes the chapter folder for `*.align.tx.json`, derives a stem with `ExtractChapterStem`, constructs hydrate/TextGrid paths, and picks the first existing audio file from a treated/pause-adjusted fallback list. It short-circuits with `Log.Debug` and `null` when required transcript or audio inputs are missing, otherwise computes audio metrics via `ComputeAudioStats`. If `bookIndex` and hydrate data are present, it loads transcript/hydrated JSON, reads MFA silences, resolves pause policy, and calls `PauseMapBuilder.Build(..., includeAllIntraSentenceGaps: true)` to populate prosody stats; failures in this branch are caught and only logged at debug level.


#### [[PipelineCommand.ComputeChapterStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static PipelineCommand.ChapterStats ComputeChapterStats(DirectoryInfo chapterDir, BookIndex bookIndex, FileInfo bookIndexFile)
```

**Calls ->**
- [[PipelineCommand.ComputeAudioStats]]
- [[PipelineCommand.ExtractChapterStem]]
- [[PipelineCommand.LoadJson]]
- [[PipelineCommand.LoadMfaSilences]]
- [[PausePolicyResolver.Resolve]]
- [[Log.Debug]]
- [[PauseMapBuilder.Build]]

**Called-by <-**
- [[PipelineCommand.RunStats]]

