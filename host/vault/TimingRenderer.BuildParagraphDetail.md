---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 5
tags:
  - method
  - llm/utility
  - llm/factory
  - llm/validation
---
# TimingRenderer::BuildParagraphDetail
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Build a panelized paragraph-detail renderable that presents paragraph metrics alongside pending timing adjustments and diff data.**

`BuildParagraphDetail` is a private composition method that constructs an `IRenderable` paragraph-detail view for timing-session validation output. It retrieves detail inputs through `GetParagraphInfo` and `GetPendingAdjustments`, builds presentation sections with `CreateStatsTable` and `BuildDiffTable`, then encapsulates the result with `WrapInPanel`. With complexity 11, it likely handles multiple conditional render branches (for example, empty or partial adjustment/diff states) and is invoked by `BuildDetailAnalytics`.


#### [[TimingRenderer.BuildParagraphDetail]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IRenderable BuildParagraphDetail()
```

**Calls ->**
- [[InteractiveState.GetParagraphInfo]]
- [[InteractiveState.GetPendingAdjustments]]
- [[TimingRenderer.BuildDiffTable]]
- [[TimingRenderer.CreateStatsTable]]
- [[TimingRenderer.WrapInPanel]]

**Called-by <-**
- [[TimingRenderer.BuildDetailAnalytics]]

