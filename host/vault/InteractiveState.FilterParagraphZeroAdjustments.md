---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::FilterParagraphZeroAdjustments
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Remove paragraph-zero pause adjustments from the mutable adjustments collection prior to building the final adjustment set.**

`FilterParagraphZeroAdjustments` performs an in-place pass over the provided `List<PauseAdjust>` and applies `IsParagraphZero` to each adjustment to filter out paragraph-0 entries before downstream processing. Its low cyclomatic complexity (3) suggests a simple loop-plus-branch implementation with no async behavior or persistence concerns. Because it returns `void`, it communicates results by mutating the input list consumed by `BuildAdjustmentsIncludingStatic`.


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

