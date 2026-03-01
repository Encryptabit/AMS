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
  - llm/factory
---
# InteractiveState::GetCompressionControlsSnapshot
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Return a point-in-time snapshot of compression control settings from the interactive validation state.**

`GetCompressionControlsSnapshot()` is a thin accessor that delegates to `GetSnapshot` to capture the current compression-control UI/state into a `CompressionControlsSnapshot` DTO. The low cyclomatic complexity indicates no branching-heavy logic, with behavior centered on retrieving and returning the snapshot object for downstream rendering. In this class, it serves as the state-export point consumed by `BuildOptionsPanel`.


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

