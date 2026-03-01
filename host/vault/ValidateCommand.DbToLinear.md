---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# ValidateCommand::DbToLinear
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Convert a decibel value to its linear amplitude multiplier.**

`DbToLinear` is a private static, branchless O(1) helper that converts a dB scalar into linear gain via a single mathematical expression (typically `Math.Pow(10d, db / 20d)`). It has no side effects or control-flow complexity and is used by `ComputeToneGainLinear` to move from decibel-domain configuration into amplitude-domain math.


#### [[ValidateCommand.DbToLinear]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double DbToLinear(double db)
```

**Called-by <-**
- [[ValidateCommand.ComputeToneGainLinear]]

