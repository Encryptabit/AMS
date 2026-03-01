---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::ResolveVerifyTargets
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Select the effective set of chapter WAV targets to verify based on explicit chapter input, REPL context, and fallback directory scanning.**

`ResolveVerifyTargets` computes which chapter WAV files `RunVerify` should process by prioritizing explicit user input, then REPL state, then filesystem discovery. If `chapterName` is provided, it normalizes to a `.wav` filename, checks existence under `root`, logs `Debug` and returns an empty list when missing, otherwise returns a single `FileInfo`. Without `chapterName`, it checks `ReplContext.Current` and uses `PathsEqual` (full-path comparison with OS-specific case sensitivity) to ensure the REPL directory matches `root`, then selects all REPL chapters (`verifyAll || RunAllChapters`), the active chapter, or none; otherwise it enumerates top-level `*.wav`, sorts by `file.Name` with `PathComparer`, and returns either all files or the first one.


#### [[PipelineCommand.ResolveVerifyTargets]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<FileInfo> ResolveVerifyTargets(DirectoryInfo root, string chapterName, bool verifyAll)
```

**Calls ->**
- [[PipelineCommand.PathsEqual]]
- [[Log.Debug]]

**Called-by <-**
- [[PipelineCommand.RunVerify]]

