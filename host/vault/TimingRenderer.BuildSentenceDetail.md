---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/utility
  - llm/factory
---
# TimingRenderer::BuildSentenceDetail
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Build and return the sentence-detail analytics panel for a timing session by aggregating diff, stats, and pending-adjustment renderables.**

`BuildSentenceDetail()` constructs an `IRenderable` by composing sentence-level UI fragments: it generates a diff table via `BuildDiffTable`, creates stats via `CreateStatsTable`, incorporates pending adjustment data from `GetPendingAdjustments`, and finalizes layout with `WrapInPanel`. With complexity 4, the implementation likely contains only light conditional branching around section inclusion/content shaping. It serves as a focused renderer helper in `TimingRenderer` and is consumed by `BuildDetailAnalytics`.


#### [[TimingRenderer.BuildSentenceDetail]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IRenderable BuildSentenceDetail()
```

**Calls ->**
- [[InteractiveState.GetPendingAdjustments]]
- [[TimingRenderer.BuildDiffTable]]
- [[TimingRenderer.CreateStatsTable]]
- [[TimingRenderer.WrapInPanel]]

**Called-by <-**
- [[TimingRenderer.BuildDetailAnalytics]]

