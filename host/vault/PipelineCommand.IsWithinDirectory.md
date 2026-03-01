---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PipelineCommand::IsWithinDirectory
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Determines whether a candidate filesystem path should be treated as inside an expected normalized directory root.**

`IsWithinDirectory` is a `private static` boolean helper in `Ams.Cli.Commands.PipelineCommand` that takes `candidatePath` and a pre-normalized root (`directoryNormalized`) and returns a containment verdict used as a guard in `CreatePrepStageCommand`. With cyclomatic complexity `1`, it is implemented as a single straightforward predicate/return path rather than branching workflow logic. Its role is path-scope validation at command-construction time.


#### [[PipelineCommand.IsWithinDirectory]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsWithinDirectory(string candidatePath, string directoryNormalized)
```

**Called-by <-**
- [[PipelineCommand.CreatePrepStageCommand]]

