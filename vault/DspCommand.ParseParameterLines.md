---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 20
fan_in: 1
fan_out: 0
tags:
  - method
  - danger/high-complexity
---
# DspCommand::ParseParameterLines
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`

> [!danger] High Complexity (20)
> Cyclomatic complexity: 20. Consider refactoring into smaller methods.


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

