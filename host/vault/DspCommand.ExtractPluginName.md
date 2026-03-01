---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# DspCommand::ExtractPluginName
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Best-effort parse of plugalyzer stdout to derive a plugin name from a `Loaded plugin` line.**

`ExtractPluginName` iterates the provided output lines, skipping whitespace-only entries, and stops at the first line that contains `Loaded plugin` using case-insensitive matching. It extracts the quoted plugin name via `Regex.Match(raw, "Loaded plugin\\s+\"(?<name>.+?)\"")`, then falls back to `raw.Split('"')` when regex capture fails. If parsing is still ambiguous it returns the trimmed source line, and if no matching line exists it returns `null` for the caller (`CreateInitCommand`) to persist as optional metadata.


#### [[DspCommand.ExtractPluginName]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ExtractPluginName(IReadOnlyList<string> lines)
```

**Called-by <-**
- [[DspCommand.CreateInitCommand]]

