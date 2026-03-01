---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/utility
---
# PipelineProgressReporter::ReportStage
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Propagate a pipeline stage/status update for a chapter through the reporter’s common progress-update path.**

`ReportStage` in `PipelineCommand.PipelineProgressReporter` is a one-line adapter that forwards `chapterId`, `stage`, and `message` to the shared `Update` routine. The method itself has no branching or state logic (complexity 1), so task existence checks, synchronization, clamping, and display text updates are centralized in `Update`.


#### [[PipelineProgressReporter.ReportStage]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void ReportStage(string chapterId, PipelineStage stage, string message)
```

**Calls ->**
- [[PipelineProgressReporter.Update]]

