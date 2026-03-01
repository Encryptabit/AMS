---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
---
# CompressionState::IsWithinScope
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[CompressionState.IsWithinScope]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private bool IsWithinScope(ValidateTimingSession.EditablePause pause)
```

**Calls ->**
- [[CompressionState.GetPauseParagraphId]]

**Called-by <-**
- [[CompressionState.ApplyPreview]]

