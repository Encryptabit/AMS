---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# DspCommand::PrintAstatsLogs
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Formats and prints only astats-related diagnostic log lines (or warns when none exist) for `ExecuteFilterChain` output.**

`PrintAstatsLogs` is a static CLI helper that gates output of captured filter-graph logs to astats-relevant lines. It first checks `logs.Count` and emits `Log.Warn("astats did not emit any log lines.")` before returning when empty. Otherwise it prints an `astats output` header and iterates each line, writing only entries whose text case-insensitively contains `AStats`, `Overall`, `Max level`, `Min level`, `DC offset`, or `RMS`, escaping markup via `Markup.Escape` before `AnsiConsole.MarkupLine`.


#### [[DspCommand.PrintAstatsLogs]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void PrintAstatsLogs(IReadOnlyList<string> logs)
```

**Calls ->**
- [[Log.Warn]]

**Called-by <-**
- [[DspCommand.ExecuteFilterChain]]

