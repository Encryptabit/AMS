---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 7
fan_in: 2
fan_out: 5
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::EnsureCompressionStateForCurrentScope
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Keeps compression-related interactive state (preview and selection) synchronized with the current validation scope and policy.**

EnsureCompressionStateForCurrentScope reconciles interactive compression state against the active timing scope. It collects pause ranges with `CollectCompressionPauses`, derives the expected compression model from policy via `FromPolicy`, and uses `MatchesScope` to avoid unnecessary work when the current state is still valid. If scope/policy no longer align, it regenerates the preview through `RebuildPreview` and optionally clears selection with `ResetSelection` when `resetSelection` is true.


#### [[InteractiveState.EnsureCompressionStateForCurrentScope]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void EnsureCompressionStateForCurrentScope(bool resetSelection)
```

**Calls ->**
- [[InteractiveState.CollectCompressionPauses]]
- [[CompressionControls.FromPolicy]]
- [[CompressionState.MatchesScope]]
- [[CompressionState.RebuildPreview]]
- [[CompressionState.ResetSelection]]

**Called-by <-**
- [[InteractiveState.RefreshCompressionStateIfNeeded]]
- [[InteractiveState.ToggleOptionsFocus]]

