---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
---
# NullProgressReporter::ReportStage
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Provide a Null Object implementation of stage reporting so callers can invoke progress updates without conditional logic.**

`NullProgressReporter.ReportStage` is a no-op implementation inside `PipelineCommand.CompactPipelineProgressReporter.NullProgressReporter`. The method body is empty, so the `chapterId`, `stage`, and `message` parameters are intentionally ignored and no state, output, or side effects occur.


#### [[NullProgressReporter.ReportStage]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void ReportStage(string chapterId, PipelineStage stage, string message)
```

