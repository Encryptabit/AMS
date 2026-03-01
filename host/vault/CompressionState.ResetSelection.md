---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# CompressionState::ResetSelection
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**It restores the compression interaction state to its default selection and preview offset for the current scope.**

CompressionState.ResetSelection() is an O(1) state-reset method that unconditionally assigns SelectedControlIndex = 0 and PreviewOffset = 0, with no branching, allocation, or I/O. It is called by EnsureCompressionStateForCurrentScope(bool resetSelection) when the existing compression state still matches the current scope and the caller requests a reset, immediately before RebuildPreview(_basePolicy).


#### [[CompressionState.ResetSelection]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void ResetSelection()
```

**Called-by <-**
- [[InteractiveState.EnsureCompressionStateForCurrentScope]]

