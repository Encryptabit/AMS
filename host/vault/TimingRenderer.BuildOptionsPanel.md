---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 16
fan_in: 1
fan_out: 4
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/factory
---
# TimingRenderer::BuildOptionsPanel
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

> [!danger] High Complexity (16)
> Cyclomatic complexity: 16. Consider refactoring into smaller methods.

## Summary
**Construct the timing session’s options panel UI from current chapter, compression, and pause state for inclusion in the overall layout.**

`BuildOptionsPanel` is a private `TimingRenderer` composition helper that builds an `IRenderable` options section by combining chapter context (`GetChapterEntry`), current compression-control state (`GetCompressionControlsSnapshot`), a computed compression preview (`GetCompressionPreview`), and pending pause metrics (`GetPendingPauseCount`). It is called by `BuildLayout`, indicating the method centralizes options-panel assembly while delegating state retrieval and preview computation to focused helper methods.


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

