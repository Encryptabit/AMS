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
# TimingRenderer::BuildSentenceDetail
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[TimingRenderer.BuildSentenceDetail]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IRenderable BuildSentenceDetail()
```

**Calls ->**
- [[InteractiveState.GetPendingAdjustments]]
- [[TimingRenderer.BuildDiffTable]]
- [[TimingRenderer.CreateStatsTable]]
- [[TimingRenderer.WrapInPanel]]

**Called-by <-**
- [[TimingRenderer.BuildDetailAnalytics]]

