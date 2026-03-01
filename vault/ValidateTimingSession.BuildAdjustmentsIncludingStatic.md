---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 4
tags:
  - method
---
# ValidateTimingSession::BuildAdjustmentsIncludingStatic
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[ValidateTimingSession.BuildAdjustmentsIncludingStatic]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IReadOnlyList<PauseAdjust> BuildAdjustmentsIncludingStatic(ValidateTimingSession.InteractiveState state)
```

**Calls ->**
- [[ValidateTimingSession.BuildStaticBufferAdjustments]]
- [[InteractiveState.FilterParagraphZeroAdjustments]]
- [[InteractiveState.GetCommittedAdjustments]]
- [[ValidateTimingSession.IsStructuralClass]]

**Called-by <-**
- [[ValidateTimingSession.PersistPauseAdjustments]]

