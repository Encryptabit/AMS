---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::ApplyCompressionPreview
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Apply the current compression preview state and return a typed summary of what was applied.**

`ApplyCompressionPreview` is a thin wrapper in `InteractiveState` that produces a `ValidateTimingSession.CompressionApplySummary` by delegating to the shared `ApplyPreview` path for compression-related changes. With cyclomatic complexity 2, it keeps local control flow minimal and relies on `ApplyPreview` for core behavior. Its placement in the call graph (`CommitCurrentScope`, `RunHeadlessAsync`) indicates it is the common compression-apply hook used in both interactive commit and headless execution flows.


#### [[InteractiveState.ApplyCompressionPreview]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ValidateTimingSession.CompressionApplySummary ApplyCompressionPreview()
```

**Calls ->**
- [[CompressionState.ApplyPreview]]

**Called-by <-**
- [[ValidateTimingSession.RunHeadlessAsync]]
- [[TimingController.CommitCurrentScope]]

