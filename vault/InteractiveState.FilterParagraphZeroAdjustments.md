---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# InteractiveState::FilterParagraphZeroAdjustments
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.FilterParagraphZeroAdjustments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void FilterParagraphZeroAdjustments(List<PauseAdjust> adjustments)
```

**Calls ->**
- [[InteractiveState.IsParagraphZero]]

**Called-by <-**
- [[ValidateTimingSession.BuildAdjustmentsIncludingStatic]]

