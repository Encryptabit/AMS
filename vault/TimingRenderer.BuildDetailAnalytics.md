---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 4
tags:
  - method
---
# TimingRenderer::BuildDetailAnalytics
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


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

