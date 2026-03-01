---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 4
tags:
  - method
---
# TimingRenderer::BuildTree
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


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

