---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PipelineCommand::EnsureTrailingSeparator
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Ensure a path string ends with a directory separator so directory-prefix matching and replacement logic behave consistently.**

In `Ams.Cli.Commands.PipelineCommand`, `EnsureTrailingSeparator` normalizes a path for prefix-based directory remapping by guaranteeing a trailing separator. It checks `path.EndsWith(Path.DirectorySeparatorChar)` and `path.EndsWith(Path.AltDirectorySeparatorChar)`; if either matches, it returns the input unchanged, otherwise it returns `path + Path.DirectorySeparatorChar`. This keeps already-normalized inputs allocation-free and only allocates when a separator must be appended.


#### [[PipelineCommand.EnsureTrailingSeparator]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string EnsureTrailingSeparator(string path)
```

**Called-by <-**
- [[PipelineCommand.ProjectPath]]

