---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PipelineCommand::ExtractUnmatchedParts
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Extract ordered word-like unmatched segments from a chapter filename stem so rename patterns can reference them.**

`ExtractUnmatchedParts` uses a static compiled regex (`[A-Za-z]+[A-Za-z0-9]*`) to pull alpha-leading alphanumeric tokens from `stem`, preserving match order for downstream rename token substitution. It returns `Array.Empty<string>()` for null/whitespace input and for zero matches to avoid unnecessary allocation. When matches exist, it allocates a `string[]` of exact size and copies each `Match.Value`; `CreatePrepRenameCommand` then feeds this into `ApplyRenamePattern` for `{um...}` expansions.


#### [[PipelineCommand.ExtractUnmatchedParts]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string[] ExtractUnmatchedParts(string stem)
```

**Called-by <-**
- [[PipelineCommand.CreatePrepRenameCommand]]

