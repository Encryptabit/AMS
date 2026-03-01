---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 16
fan_in: 1
fan_out: 4
tags:
  - method
  - danger/high-complexity
---
# TimingRenderer::BuildOptionsPanel
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

> [!danger] High Complexity (16)
> Cyclomatic complexity: 16. Consider refactoring into smaller methods.


#### [[TimingRenderer.BuildOptionsPanel]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IRenderable BuildOptionsPanel()
```

**Calls ->**
- [[InteractiveState.GetChapterEntry]]
- [[InteractiveState.GetCompressionControlsSnapshot]]
- [[InteractiveState.GetCompressionPreview]]
- [[InteractiveState.GetPendingPauseCount]]

**Called-by <-**
- [[TimingRenderer.BuildLayout]]

