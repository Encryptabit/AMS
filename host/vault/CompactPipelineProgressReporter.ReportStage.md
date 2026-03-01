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
  - llm/data-access
  - llm/utility
---
# CompactPipelineProgressReporter::ReportStage
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Set the tracked chapter’s current pipeline stage and human-readable progress message.**

This public reporter callback delegates directly to UpdateChapter(chapterId, status => ...), where the lambda assigns status.Stage = stage and status.Message = message. It performs no validation, branching, or formatting of inputs (complexity 1). Any unknown-chapter no-op behavior, thread synchronization, and live display refresh are provided by UpdateChapter rather than this method.


#### [[CompactPipelineProgressReporter.ReportStage]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void ReportStage(string chapterId, PipelineStage stage, string message)
```

**Calls ->**
- [[CompactPipelineProgressReporter.UpdateChapter]]

