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
# PipelineCommand::EscapeCsv
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Convert a raw string into a CSV-safe field by conditionally quoting and escaping embedded double quotes.**

`EscapeCsv` performs minimal CSV field escaping: it returns `string.Empty` for null/empty input, checks for delimiter/control characters with `IndexOfAny(new[] { ',', '"', '\n', '\r' })`, and fast-path returns the original value when quoting is unnecessary. When escaping is required, it doubles embedded quotes via `value.Replace("\"", "\"\"", StringComparison.Ordinal)` and wraps the result in outer quotes. `WriteVerificationCsv` uses it for string columns (`chapterLabel`, `variantLabel`, `sentenceIds`) before `string.Join(',')` output.


#### [[PipelineCommand.EscapeCsv]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string EscapeCsv(string value)
```

**Called-by <-**
- [[PipelineCommand.WriteVerificationCsv]]

