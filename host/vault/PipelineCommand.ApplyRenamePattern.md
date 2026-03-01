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
  - llm/error-handling
---
# PipelineCommand::ApplyRenamePattern
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Expand a rename pattern into a concrete file-name stem using chapter index and extracted unmatched name parts.**

`ApplyRenamePattern` uses `PatternTokenRegex` and a `Regex.Replace` evaluator to expand rename placeholders in a template string, and is used by `CreatePrepRenameCommand` to compute new chapter stems. It supports numeric index tokens (`{d}`, `{dd}` with optional signed offsets) and unmatched-part tokens (`{um#}`, `{um#-#}`, `{um*}`), where unmatched indexes are 1-based and range/all results are joined with underscores. The method enforces token validity by throwing `InvalidOperationException` for malformed unmatched tokens/ranges, disallowed offsets on `um` tokens, and any leftover `{`/`}` indicating unsupported placeholders.


#### [[PipelineCommand.ApplyRenamePattern]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ApplyRenamePattern(string pattern, int index, IReadOnlyList<string> unmatchedParts)
```

**Called-by <-**
- [[PipelineCommand.CreatePrepRenameCommand]]

