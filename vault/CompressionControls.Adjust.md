---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 9
fan_in: 1
fan_out: 4
tags:
  - method
---
# CompressionControls::Adjust
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[CompressionControls.Adjust]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool Adjust(int index, double deltaMultiplier)
```

**Calls ->**
- [[CompressionControls.SetKneeWidth]]
- [[CompressionControls.SetPreserveTopQuantile]]
- [[CompressionControls.SetRatioInside]]
- [[CompressionControls.SetRatioOutside]]

**Called-by <-**
- [[CompressionState.AdjustSelectedControl]]

