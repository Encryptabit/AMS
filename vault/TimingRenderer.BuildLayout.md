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
# TimingRenderer::BuildLayout
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


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

