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
---
# PipelineCommand::GetStagedFileName
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Builds the prep-stage output filename by removing known staging suffixes from the source basename while keeping the original file extension.**

`GetStagedFileName` normalizes staged audio names by reading `Path.GetFileNameWithoutExtension(source.Name)` and iteratively stripping trailing pipeline markers from a fixed list (`.pause-adjusted`, `.treated`). It removes both dotted and non-dotted suffix variants with `EndsWith(..., PathComparison)` so matching follows OS-specific case sensitivity, and guards against over-trimming by breaking when the marker length would consume the whole stem. It then returns the cleaned stem plus the original `source.Extension`.


#### [[PipelineCommand.GetStagedFileName]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string GetStagedFileName(FileInfo source)
```

**Called-by <-**
- [[PipelineCommand.CreatePrepStageCommand]]

