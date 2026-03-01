---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 5
tags:
  - method
---
# TimingRenderer::BuildChapterDetail
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


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

