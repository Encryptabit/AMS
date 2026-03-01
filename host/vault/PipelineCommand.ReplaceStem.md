---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# PipelineCommand::ReplaceStem
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**It rewrites the leading stem of a name in a case-insensitive way for rename planning, leaving non-matching names intact.**

`ReplaceStem` implements a prefix-only rename by testing `name.StartsWith(oldStem, StringComparison.OrdinalIgnoreCase)` and, if true, returning `newStem + name[oldStem.Length..]`; otherwise it returns `name` unchanged. Using the range slice preserves the original suffix and swaps only the leading stem. `CollectRenameOperations` calls it to compute renamed directory and file names when building rename operations.


#### [[PipelineCommand.ReplaceStem]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ReplaceStem(string name, string oldStem, string newStem)
```

**Called-by <-**
- [[PipelineCommand.CollectRenameOperations]]

