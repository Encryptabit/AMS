---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::LoadJson
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Load JSON from a file into a typed model used by the pipeline’s stats commands.**

`LoadJson<T>(FileInfo file)` is a private static generic helper on `Ams.Cli.Commands.PipelineCommand` that centralizes file-based JSON deserialization from a `FileInfo` source into a strongly typed `T`. Given complexity 2, the implementation is likely a single main deserialization path plus one guard/failure branch (for invalid input, missing file, or deserialize failure). It is called by `ComputeChapterStats` and `RunStats`, so those flows rely on it as the shared boundary for loading persisted stats/config payloads.


#### [[PipelineCommand.LoadJson]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static T LoadJson<T>(FileInfo file)
```

**Called-by <-**
- [[PipelineCommand.ComputeChapterStats]]
- [[PipelineCommand.RunStats]]

