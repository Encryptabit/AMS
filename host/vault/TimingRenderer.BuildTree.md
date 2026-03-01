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
# TimingRenderer::BuildTree
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Creates the viewport-aware timing tree renderable used by the layout pipeline.**

BuildTree is a synchronous renderer helper that constructs an `IRenderable` tree for the timing view and is consumed by `BuildLayout`. It applies viewport constraints via `SetTreeViewportSize(viewportHeight)`, gathers visible nodes with `GetTreeViewportEntries`, computes summary content through `BuildTreeSummary`, and formats display labels through `FormatTreeLabel` before assembling the final tree output. The method’s low complexity reflects stepwise orchestration of rendering helpers rather than embedded business logic.


#### [[TimingRenderer.BuildTree]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IRenderable BuildTree(int viewportHeight)
```

**Calls ->**
- [[InteractiveState.GetTreeViewportEntries]]
- [[InteractiveState.SetTreeViewportSize]]
- [[TimingRenderer.BuildTreeSummary]]
- [[TimingRenderer.FormatTreeLabel]]

**Called-by <-**
- [[TimingRenderer.BuildLayout]]

