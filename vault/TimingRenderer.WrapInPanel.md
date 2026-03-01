---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 1
fan_in: 4
fan_out: 0
tags:
  - method
---
# TimingRenderer::WrapInPanel
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[TimingRenderer.WrapInPanel]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Panel WrapInPanel(IRenderable content, string title)
```

**Called-by <-**
- [[TimingRenderer.BuildChapterDetail]]
- [[TimingRenderer.BuildParagraphDetail]]
- [[TimingRenderer.BuildPauseDetail]]
- [[TimingRenderer.BuildSentenceDetail]]

