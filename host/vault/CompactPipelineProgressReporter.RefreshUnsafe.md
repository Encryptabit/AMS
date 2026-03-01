---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# CompactPipelineProgressReporter::RefreshUnsafe
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Refresh the in-place pipeline progress UI with the latest computed view after state transitions.**

`RefreshUnsafe` is a thin helper that rebuilds the progress table by calling `BuildView()` and pushes it to the live Spectre.Console surface with `_liveContext?.UpdateTarget(...)`. It does not lock or mutate state itself, so synchronization is delegated to callers (`UpdateChapter` and `MarkFinished`), and the null-conditional safely no-ops before `Attach` sets `_liveContext`.


#### [[CompactPipelineProgressReporter.RefreshUnsafe]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void RefreshUnsafe()
```

**Calls ->**
- [[CompactPipelineProgressReporter.BuildView]]

**Called-by <-**
- [[CompactPipelineProgressReporter.MarkFinished]]
- [[CompactPipelineProgressReporter.UpdateChapter]]

