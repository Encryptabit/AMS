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
# PipelineCommand::NormalizeVariantToken
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Converts an arbitrary variant string into a lowercase, dash-delimited safe token with a default fallback value.**

`NormalizeVariantToken` sanitizes a variant label into a stable filename token used by `RunVerify` when emitting `.verify.{variant}.json/.csv` artifacts. The method first guards `null`/whitespace and returns `"variant"`, then applies `ToLowerInvariant()`, maps each non-alphanumeric character to `'-'` via LINQ, and trims edge dashes. If the transformed token is empty (for inputs like punctuation-only strings), it falls back to `"variant"` again.


#### [[PipelineCommand.NormalizeVariantToken]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeVariantToken(string variant)
```

**Called-by <-**
- [[PipelineCommand.RunVerify]]

