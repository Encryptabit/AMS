---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
---
# PipelineCommand::ResolveRenameTargets
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Selects which chapter WAV files should be targeted by the rename operation, using REPL context when available and filesystem discovery otherwise.**

`ResolveRenameTargets` determines rename candidates for `CreatePrepRenameCommand` by preferring REPL-scoped selection unless `forceAll` is set. If `ReplContext.Current` exists and its `WorkingDirectory` matches `root` via `PathsEqual`, it returns either `ActiveChapter` as a single-item list, `repl.Chapters.ToList()`, or an empty list depending on `RunAllChapters` and active-chapter state. If REPL scoping is bypassed or not applicable, it enumerates top-level `*.wav` files under `root`, projects them to `FileInfo`, orders by filename with `StringComparer.OrdinalIgnoreCase`, and materializes a `List<FileInfo>`.


#### [[PipelineCommand.ResolveRenameTargets]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<FileInfo> ResolveRenameTargets(DirectoryInfo root, bool forceAll = false)
```

**Calls ->**
- [[PipelineCommand.PathsEqual]]

**Called-by <-**
- [[PipelineCommand.CreatePrepRenameCommand]]

