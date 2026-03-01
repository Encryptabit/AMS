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
# CompressionState::MoveControlSelection
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Moves the currently selected compression control by a relative offset and reports whether the selection changed.**

`MoveControlSelection(int delta)` in `InteractiveState.CompressionState` appears to implement index-based navigation for compression controls, applying the signed `delta` to the current selection. With low cyclomatic complexity (3) and a `bool` return, the implementation is likely a small bounds-check/update flow: compute next index, reject out-of-range/no-op moves, and return whether state changed.


#### [[CompressionState.MoveControlSelection]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool MoveControlSelection(int delta)
```

**Called-by <-**
- [[InteractiveState.MoveCompressionControlSelection]]

