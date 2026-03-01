---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# CompressionState::GetPreviewSlice
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Return the current paged slice of compression preview rows and indicate whether more rows exist before or after the returned window.**

`GetPreviewSlice` derives a bounded window over the internal `Preview` list using `PreviewOffset` and `maxRows`. It guard-returns `Array.Empty<CompressionPreviewItem>()` when there is no preview data or `maxRows <= 0`, clamps the start index with `Math.Clamp`, computes slice size with `Math.Min`, then sets `hasPrevious`/`hasNext` from slice boundaries. The method finally returns `Preview.GetRange(start, count)`, and is used by `GetCompressionPreview` to drive paged preview navigation.


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

