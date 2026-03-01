---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
---
# InteractiveState::GetCommittedAdjustments
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Return the interactive session’s committed pause adjustments for downstream adjustment assembly.**

GetCommittedAdjustments() on InteractiveState is a constant-time accessor (complexity 1) that returns the committed PauseAdjust set as IReadOnlyList<PauseAdjust> without transformation, filtering, or validation. Its caller, BuildAdjustmentsIncludingStatic, uses this as the committed interactive baseline before combining with static adjustments.


#### [[InteractiveState.GetCommittedAdjustments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<PauseAdjust> GetCommittedAdjustments()
```

**Called-by <-**
- [[ValidateTimingSession.BuildAdjustmentsIncludingStatic]]

