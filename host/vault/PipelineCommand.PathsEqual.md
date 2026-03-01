---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 6
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::PathsEqual
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Checks whether two input paths resolve to the same full path under OS-appropriate string comparison rules.**

`PathsEqual` is a private static helper that returns `false` if either argument is `null`, then canonicalizes both paths with `Path.GetFullPath` and compares them using the class-level `PathComparison`. `PathComparison` is OS-dependent (`OrdinalIgnoreCase` on Windows, `Ordinal` otherwise), so path equality follows platform case-sensitivity behavior. It is side-effect free and reused by rename/target resolution code paths to keep path matching consistent.


#### [[PipelineCommand.PathsEqual]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool PathsEqual(string left, string right)
```

**Called-by <-**
- [[PipelineCommand.CollectRenameOperations]]
- [[PipelineCommand.CreatePrepRenameCommand]]
- [[PipelineCommand.ProjectPath]]
- [[PipelineCommand.ResolveRenameTargets]]
- [[PipelineCommand.ResolveVerifyTargets]]
- [[PipelineCommand.ValidateRenamePlans]]

