---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 3
tags:
  - method
---
# TimingRenderer::BuildPauseDetail
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[TimingRenderer.BuildPauseDetail]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private IRenderable BuildPauseDetail()
```

**Calls ->**
- [[InteractiveState.DescribePauseContext]]
- [[TimingRenderer.DescribePolicyWindow]]
- [[TimingRenderer.WrapInPanel]]

**Called-by <-**
- [[TimingRenderer.BuildDetailAnalytics]]

