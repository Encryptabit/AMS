---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/data-access
---
# PipelineCommand::LooksLikeChapterDirectory
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Determine whether a given filesystem directory should be treated as a chapter directory for soft-reset logic.**

`LooksLikeChapterDirectory(DirectoryInfo directory)` is a private static boolean predicate that encapsulates chapter-folder detection behind a single decision point, with multiple branches (complexity 6) indicating several heuristic checks against the provided `DirectoryInfo`. It is used by `PerformSoftReset`, so its return value acts as a guard/filter for which directories are treated as chapter targets during reset flow.


#### [[PipelineCommand.LooksLikeChapterDirectory]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool LooksLikeChapterDirectory(DirectoryInfo directory)
```

**Called-by <-**
- [[PipelineCommand.PerformSoftReset]]

