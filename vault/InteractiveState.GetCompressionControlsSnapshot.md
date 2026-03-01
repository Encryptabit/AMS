---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# InteractiveState::GetCompressionControlsSnapshot
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.GetCompressionControlsSnapshot]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ValidateTimingSession.InteractiveState.CompressionControlsSnapshot GetCompressionControlsSnapshot()
```

**Calls ->**
- [[CompressionState.GetSnapshot]]

**Called-by <-**
- [[TimingRenderer.BuildOptionsPanel]]

