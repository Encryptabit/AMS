---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# PipelineCommand::ProjectPath
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Compute the effective final path for a file after applying a sequence of planned directory rename operations.**

`ProjectPath` simulates directory renames against an input path by iterating `directoryOps` and updating a mutable `projected` string. For each op, it first checks whether `projected` falls under `op.Source` using `EnsureTrailingSeparator(op.Source)` plus `StartsWith(..., PathComparison)` and, if so, rewrites that prefix to `EnsureTrailingSeparator(op.Target)` while preserving the remaining suffix via slicing. If not a subtree match, it falls back to exact-path remapping with `PathsEqual(projected, op.Source)`; the resulting projected path is returned for later duplicate-target validation.


#### [[PipelineCommand.ProjectPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ProjectPath(string path, IReadOnlyList<PipelineCommand.RenameOp> directoryOps)
```

**Calls ->**
- [[PipelineCommand.EnsureTrailingSeparator]]
- [[PipelineCommand.PathsEqual]]

**Called-by <-**
- [[PipelineCommand.ValidateRenamePlans]]

