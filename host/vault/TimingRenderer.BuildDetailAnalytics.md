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
  - llm/utility
  - llm/factory
  - llm/validation
---
# TimingRenderer::BuildDetailAnalytics
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Assemble the detailed timing-analytics renderable by aggregating chapter, paragraph, pause, and sentence detail components for layout rendering.**

`BuildDetailAnalytics()` composes a single `IRenderable` analytics section that is consumed by `BuildLayout`, delegating content creation to `BuildChapterDetail`, `BuildParagraphDetail`, `BuildPauseDetail`, and `BuildSentenceDetail`. Its low complexity (5) indicates lightweight orchestration logic, likely focused on ordering/including these detail renderables rather than performing heavy computation.


#### [[TimingRenderer.BuildDetailAnalytics]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IRenderable BuildDetailAnalytics()
```

**Calls ->**
- [[TimingRenderer.BuildChapterDetail]]
- [[TimingRenderer.BuildParagraphDetail]]
- [[TimingRenderer.BuildPauseDetail]]
- [[TimingRenderer.BuildSentenceDetail]]

**Called-by <-**
- [[TimingRenderer.BuildLayout]]

