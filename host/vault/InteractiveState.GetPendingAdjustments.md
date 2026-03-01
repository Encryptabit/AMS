---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 6
fan_in: 3
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::GetPendingAdjustments
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Compute the set of pending timing-adjustment diff rows for a specific validation scope.**

`GetPendingAdjustments` assembles an `IReadOnlyList<ValidateTimingSession.DiffRow>` for a given `ScopeEntry` by first collecting candidate pauses with `CollectPauses(scope)` and then conditionally materializing rows via `TryCreateDiffRow`. The method’s moderate branching (complexity 6) suggests it filters/guards candidates rather than doing heavy transformation, producing only valid pending adjustments. Its output is reused by `BuildChapterDetail`, `BuildParagraphDetail`, and `BuildSentenceDetail`, so it functions as the shared normalization step for detail rendering.


#### [[InteractiveState.GetPendingAdjustments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<ValidateTimingSession.DiffRow> GetPendingAdjustments(ValidateTimingSession.ScopeEntry scope)
```

**Calls ->**
- [[InteractiveState.CollectPauses]]
- [[InteractiveState.TryCreateDiffRow]]

**Called-by <-**
- [[TimingRenderer.BuildChapterDetail]]
- [[TimingRenderer.BuildParagraphDetail]]
- [[TimingRenderer.BuildSentenceDetail]]

