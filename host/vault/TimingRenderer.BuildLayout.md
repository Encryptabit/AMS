---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/factory
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# TimingRenderer::BuildLayout
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Builds the complete terminal UI layout for the timing session renderer from runtime state and console sizing.**

`BuildLayout` assembles a Spectre.Console `Layout` hierarchy with a fixed-height header and a three-column body (`tree`, `detail`, `manuscript`), then populates sections via `BuildTree(viewportHeight)`, `BuildDetailAnalytics()`, `BuildOptionsPanel()`, and `BuildManuscript()`. It special-cases empty `_state.Entries` by returning a header-only root that includes a “No pause spans” warning. It also defensively reads `Console.WindowWidth/WindowHeight` in `try/catch`, applies fallback dimensions (180x32), and clamps values to derive proportional panel widths and viewport height.


#### [[TimingRenderer.BuildLayout]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Layout BuildLayout()
```

**Calls ->**
- [[TimingRenderer.BuildDetailAnalytics]]
- [[TimingRenderer.BuildManuscript]]
- [[TimingRenderer.BuildOptionsPanel]]
- [[TimingRenderer.BuildTree]]

**Called-by <-**
- [[TimingRenderer.Render]]

