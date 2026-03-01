---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::ResolveChapterDirectories
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Resolve which chapter directories should be processed from a root folder based on explicit chapter input and `analyzeAll` selection rules.**

`ResolveChapterDirectories` builds and returns a `List<DirectoryInfo>` by first honoring an explicit `chapterName` (via `Path.Combine(root.FullName, chapterName)`), adding it only if the directory exists and otherwise logging a debug message. If no explicit chapter is provided, it enumerates only top-level subdirectories under `root`, filters with `LooksLikeChapterDirectory`, and sorts by `directory.Name` using `PathComparer`. It returns all candidates only when `analyzeAll` is true or exactly one candidate exists; when multiple candidates are found without `--all`, it logs a debug hint and returns an empty list.


#### [[PipelineCommand.ResolveChapterDirectories]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<DirectoryInfo> ResolveChapterDirectories(DirectoryInfo root, string chapterName, bool analyzeAll)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[PipelineCommand.RunStats]]

