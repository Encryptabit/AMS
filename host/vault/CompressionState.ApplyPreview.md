---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/validation
  - llm/utility
  - llm/error-handling
---
# CompressionState::ApplyPreview
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Applies a compression preview for a given `epsilon` and `PausePolicy`, updates interactive compression state, and reports the outcome as a summary object.**

`ApplyPreview` in `CompressionState` is a state-mutating helper called by `ApplyCompressionPreview` that appears to gate work with `IsWithinScope`, rebuild candidate preview data through `RebuildPreview`, and commit updated preview/session values via `Set`. It returns a `ValidateTimingSession.CompressionApplySummary` describing the result of applying (or skipping) the preview path. With complexity 5, the implementation likely uses multiple branches/guards around scope validity and preview rebuild outcomes.


#### [[CompressionState.ApplyPreview]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ValidateTimingSession.CompressionApplySummary ApplyPreview(double epsilon, PausePolicy basePolicy)
```

**Calls ->**
- [[EditablePause.Set]]
- [[CompressionState.IsWithinScope]]
- [[CompressionState.RebuildPreview]]

**Called-by <-**
- [[InteractiveState.ApplyCompressionPreview]]

