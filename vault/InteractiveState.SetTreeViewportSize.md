---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 2
tags:
  - method
---
# InteractiveState::SetTreeViewportSize
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.SetTreeViewportSize]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SetTreeViewportSize(int size)
```

**Calls ->**
- [[InteractiveState.EnsureTreeVisibility]]
- [[InteractiveState.RefreshCompressionStateIfNeeded]]

**Called-by <-**
- [[TimingRenderer.BuildTree]]

