---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# InteractiveState::GetCompressionPreview
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Return a bounded compression preview page while indicating whether previous and next pages are available.**

This method is a thin synchronous pagination wrapper in `ValidateTimingSession.InteractiveState`: it returns an `IReadOnlyList` of `CompressionPreviewItem` and reports navigation state via `out bool hasPrevious` and `out bool hasNext`. With cyclomatic complexity 2 and a single internal call to `GetPreviewSlice`, the implementation is mostly delegation with minimal branching around preview-window boundaries, and its result is consumed by `BuildOptionsPanel`.


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

