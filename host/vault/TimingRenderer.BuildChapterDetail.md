---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 5
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/data-access
---
# TimingRenderer::BuildChapterDetail
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**BuildChapterDetail constructs the chapter detail `IRenderable` for timing-session validation by aggregating analytic tables and pending-adjustment context into a panelized output.**

BuildChapterDetail composes a chapter-level CLI detail view by creating the diff, class, and stats tables (`BuildDiffTable`, `CreateClassTable`, `CreateStatsTable`) and incorporating data from `GetPendingAdjustments`. With a complexity of 7, it likely applies conditional assembly/formatting paths before returning a single `IRenderable`. The method then normalizes presentation through `WrapInPanel`, and its output is consumed by `BuildDetailAnalytics`.


#### [[TimingRenderer.BuildChapterDetail]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IRenderable BuildChapterDetail()
```

**Calls ->**
- [[InteractiveState.GetPendingAdjustments]]
- [[TimingRenderer.BuildDiffTable]]
- [[TimingRenderer.CreateClassTable]]
- [[TimingRenderer.CreateStatsTable]]
- [[TimingRenderer.WrapInPanel]]

**Called-by <-**
- [[TimingRenderer.BuildDetailAnalytics]]

