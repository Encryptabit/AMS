---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
---
# CompressionState::GetPreviewSlice
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[CompressionState.GetPreviewSlice]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<ValidateTimingSession.InteractiveState.CompressionPreviewItem> GetPreviewSlice(int maxRows, out bool hasPrevious, out bool hasNext)
```

**Called-by <-**
- [[InteractiveState.GetCompressionPreview]]

