---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 1
tags:
  - method
---
# DspCommand::PrintAstatsLogs
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


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

