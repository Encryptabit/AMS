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
# InteractiveState::GetCompressionPreview
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[InteractiveState.GetCompressionPreview]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<ValidateTimingSession.InteractiveState.CompressionPreviewItem> GetCompressionPreview(int maxRows, out bool hasPrevious, out bool hasNext)
```

**Calls ->**
- [[CompressionState.GetPreviewSlice]]

**Called-by <-**
- [[TimingRenderer.BuildOptionsPanel]]

