---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 2
tags:
  - method
---
# TimingController::CommitCurrentScope
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[TimingController.CommitCurrentScope]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private bool CommitCurrentScope()
```

**Calls ->**
- [[InteractiveState.ApplyCompressionPreview]]
- [[InteractiveState.CommitScope]]

**Called-by <-**
- [[TimingController.Run]]

