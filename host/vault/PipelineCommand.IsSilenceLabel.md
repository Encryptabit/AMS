---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PipelineCommand::IsSilenceLabel
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Determine whether a parsed MFA label should be treated as a silence segment.**

`IsSilenceLabel` is a private static classifier used by `LoadMfaSilences` to decide whether a TextGrid interval token represents silence. The implementation first returns `true` for `null`/whitespace input, then trims and applies a length-based switch for case-insensitive matches of `"sp"` (len 2), `"sil"` (len 3), and `"<sil>"` (fallback branch). Any other token returns `false`, so only recognized silence markers pass the interval filter.


#### [[PipelineCommand.IsSilenceLabel]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsSilenceLabel(string text)
```

**Called-by <-**
- [[PipelineCommand.LoadMfaSilences]]

