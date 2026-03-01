---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
---
# InteractiveState::ApplyCompressionPreview
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.ApplyCompressionPreview]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ValidateTimingSession.CompressionApplySummary ApplyCompressionPreview()
```

**Calls ->**
- [[CompressionState.ApplyPreview]]

**Called-by <-**
- [[ValidateTimingSession.RunHeadlessAsync]]
- [[TimingController.CommitCurrentScope]]

