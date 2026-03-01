---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# InteractiveState::ScrollCompressionPreview
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Applies a scroll delta to the compression preview and reports whether the preview state changed.**

In `InteractiveState`, `ScrollCompressionPreview(int delta)` is a thin adapter used by `Run` that forwards compression-preview scroll input to `ScrollPreview`. It returns the delegated `bool` result so the interactive loop can react to state changes, with only light branching (cyclomatic complexity 3).


#### [[InteractiveState.ScrollCompressionPreview]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool ScrollCompressionPreview(int delta)
```

**Calls ->**
- [[CompressionState.ScrollPreview]]

**Called-by <-**
- [[TimingController.Run]]

