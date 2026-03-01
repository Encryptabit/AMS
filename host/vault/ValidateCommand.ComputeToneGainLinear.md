---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# ValidateCommand::ComputeToneGainLinear
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Calculate the linear gain multiplier needed to transform a signal from the seed mean RMS dB level to the target RMS dB level.**

`ComputeToneGainLinear` computes a gain multiplier from two RMS levels in dB by deriving the dB delta (`targetRmsDb - seedMeanRmsDb`) and passing that value to `DbToLinear`. The implementation is intentionally minimal (complexity 2), with no branching, no normalization logic, and no side effects. It returns a `double` linear-scale factor for downstream amplitude adjustment.


#### [[ValidateCommand.ComputeToneGainLinear]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double ComputeToneGainLinear(double seedMeanRmsDb, double targetRmsDb)
```

**Calls ->**
- [[ValidateCommand.DbToLinear]]

