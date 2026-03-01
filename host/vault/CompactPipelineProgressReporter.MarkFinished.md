---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# CompactPipelineProgressReporter::MarkFinished
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Marks compact pipeline progress reporting as finished and triggers a refresh of the displayed progress state.**

Within `PipelineCommand.CompactPipelineProgressReporter`, `MarkFinished()` is a private, synchronous terminal-state helper with cyclomatic complexity 1, so its control flow is straight-line. Its implementation path ends in `RefreshUnsafe`, coupling completion-state transition with an immediate reporter refresh. It is called only from `RunAsync`, placing it in the pipeline command’s completion/reporting phase.


#### [[CompactPipelineProgressReporter.MarkFinished]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void MarkFinished()
```

**Calls ->**
- [[CompactPipelineProgressReporter.RefreshUnsafe]]

**Called-by <-**
- [[CompactPipelineProgressReporter.RunAsync]]

