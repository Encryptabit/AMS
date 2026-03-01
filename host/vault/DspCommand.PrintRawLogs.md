---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
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
# DspCommand::PrintRawLogs
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Print raw filter-graph log output to the CLI with an empty-list guard and escaped line-by-line formatting.**

`PrintRawLogs` is a private static helper in `DspCommand` (called by `ExecuteFilterChain`) that renders filter-graph logs to Spectre.Console. It first checks `logs.Count == 0`; if empty, it prints a yellow “No filter-graph log output.” message and returns. When logs exist, it prints a bold green header and writes each line with `AnsiConsole.MarkupLine(Markup.Escape(line))`, escaping content before markup rendering.


#### [[DspCommand.PrintRawLogs]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void PrintRawLogs(IReadOnlyList<string> logs)
```

**Called-by <-**
- [[DspCommand.ExecuteFilterChain]]

