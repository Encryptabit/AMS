---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# InteractiveState::GetBaselineTimeline
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Expose baseline sentence timing data from interactive validation state for static buffer adjustment computation.**

`GetBaselineTimeline()` on `ValidateTimingSession.InteractiveState` is a low-complexity accessor that returns baseline sentence timings keyed by sentence id/index as `IReadOnlyDictionary<int, SentenceTiming>`. Given complexity 2 and its use by `BuildStaticBufferAdjustments`, the implementation is likely a thin wrapper over internal state with at most a simple guard path before returning the baseline map used for adjustment math.


#### [[InteractiveState.GetBaselineTimeline]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyDictionary<int, SentenceTiming> GetBaselineTimeline()
```

**Called-by <-**
- [[ValidateTimingSession.BuildStaticBufferAdjustments]]

