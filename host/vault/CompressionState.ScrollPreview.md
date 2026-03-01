---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# CompressionState::ScrollPreview
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Move the compression preview offset up or down within valid bounds and indicate whether the scroll changed state.**

ScrollPreview performs bounded viewport scrolling over the compression preview list by adjusting `PreviewOffset` in constant time. It short-circuits with `false` when there is no preview data or `delta == 0`, then computes `maxOffset = Math.Max(0, Preview.Count - 1)` and clamps `PreviewOffset + delta` via `Math.Clamp(...)`. If the clamped offset is unchanged it returns `false`; otherwise it updates `PreviewOffset` and returns `true` to signal state mutation.


#### [[CompressionState.ScrollPreview]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool ScrollPreview(int delta)
```

**Called-by <-**
- [[InteractiveState.ScrollCompressionPreview]]

