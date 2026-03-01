---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::GetTreeViewportEntries
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Returns the currently visible tree entries for the interactive timing-session view and reports whether additional entries exist above or below the viewport.**

`GetTreeViewportEntries` is a small viewport-selection helper in `InteractiveState` that first invokes `EnsureTreeVisibility` to keep the current tree focus/range valid, then returns the visible `ScopeEntry` slice as an `IReadOnlyList`. It also computes `hasPrevious` and `hasNext` via `out` parameters to indicate whether there are off-screen items before or after the current window, which `BuildTree` can use for paging/navigation cues.


#### [[InteractiveState.GetTreeViewportEntries]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<ValidateTimingSession.ScopeEntry> GetTreeViewportEntries(out bool hasPrevious, out bool hasNext)
```

**Calls ->**
- [[InteractiveState.EnsureTreeVisibility]]

**Called-by <-**
- [[TimingRenderer.BuildTree]]

