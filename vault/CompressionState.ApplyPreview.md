---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 3
tags:
  - method
---
# CompressionState::ApplyPreview
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[CompressionState.ApplyPreview]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ValidateTimingSession.CompressionApplySummary ApplyPreview(double epsilon, PausePolicy basePolicy)
```

**Calls ->**
- [[EditablePause.Set]]
- [[CompressionState.IsWithinScope]]
- [[CompressionState.RebuildPreview]]

**Called-by <-**
- [[InteractiveState.ApplyCompressionPreview]]

