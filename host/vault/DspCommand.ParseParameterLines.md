---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 20
fan_in: 1
fan_out: 0
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# DspCommand::ParseParameterLines
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

> [!danger] High Complexity (20)
> Cyclomatic complexity: 20. Consider refactoring into smaller methods.

## Summary
**It converts raw `listParameters` stdout into structured DSP parameter objects consumed by `CreateInitCommand` when building cached plugin metadata.**

`ParseParameterLines` iterates plugalyzer output lines and only treats lines with a leading numeric index plus colon as parameter records, skipping malformed/irrelevant lines with guard checks. For each record, it appends following indented continuation lines, then uses a regex to split inline metadata segments (`Values:`, `Default:`, `Supports text values:`) from the main parameter name text. It constructs `DspPluginParameter` instances with parsed `Index`, `Name`, optional `Values`/`Default`, and nullable boolean `SupportsTextValues`, and returns `null` when no valid parameters are found.


#### [[DspCommand.ParseParameterLines]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<DspPluginParameter> ParseParameterLines(IReadOnlyList<string> lines)
```

**Called-by <-**
- [[DspCommand.CreateInitCommand]]

