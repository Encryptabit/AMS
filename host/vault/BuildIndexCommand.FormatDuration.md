---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# BuildIndexCommand::FormatDuration
**Path**: `Projects/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs`

## Summary
**Formats a raw duration in seconds into a compact human-readable string for build index output.**

`FormatDuration` converts a `double` second count to a `TimeSpan` and formats it using three branches: hours+minutes+seconds, minutes+seconds, or seconds-only. The implementation checks `timeSpan.TotalHours` first, then `timeSpan.TotalMinutes`, and uses component fields (`Minutes`, `Seconds`) for display. In the hour case it truncates hours with `(int)timeSpan.TotalHours`, producing whole-second, non-fractional output intended for concise CLI reporting.


#### [[BuildIndexCommand.FormatDuration]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatDuration(double totalSeconds)
```

**Called-by <-**
- [[BuildIndexCommand.BuildBookIndexAsync]]

