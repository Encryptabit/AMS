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
# PipelineCommand::NormalizeDirectoryPath
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Normalizes a directory path to an absolute path with exactly one trailing platform-native separator.**

`NormalizeDirectoryPath` resolves the input with `Path.GetFullPath(path)`, trims any trailing `Path.DirectorySeparatorChar` and `Path.AltDirectorySeparatorChar` using `TrimEnd`, then appends exactly one `Path.DirectorySeparatorChar`. The method enforces a canonical absolute directory format with a guaranteed trailing separator, which its callers (`CreatePrepStageCommand`, `PerformSoftReset`) use for consistent directory-boundary and prefix-style path checks.


#### [[PipelineCommand.NormalizeDirectoryPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeDirectoryPath(string path)
```

**Called-by <-**
- [[PipelineCommand.CreatePrepStageCommand]]
- [[PipelineCommand.PerformSoftReset]]

