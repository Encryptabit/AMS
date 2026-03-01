---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ValidateTimingSession::IsStructuralClass
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Determine whether a given `PauseClass` should be treated as structural during timing-session validation and adjustment construction.**

`IsStructuralClass` is a private static boolean predicate on `PauseClass` inside `ValidateTimingSession`, used by `BuildAdjustmentsIncludingStatic` to gate structural-only adjustment logic. Given cyclomatic complexity 1, the implementation is effectively a single conditional/return expression with no branching fan-out, side effects, or external dependencies. It centralizes structural classification so the caller can remain focused on adjustment assembly.


#### [[ValidateTimingSession.IsStructuralClass]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsStructuralClass(PauseClass pauseClass)
```

**Called-by <-**
- [[ValidateTimingSession.BuildAdjustmentsIncludingStatic]]

