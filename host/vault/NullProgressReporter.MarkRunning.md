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
# NullProgressReporter::MarkRunning
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**It satisfies the progress-reporting interface by safely discarding “running” notifications when compact/null reporting is in use.**

`NullProgressReporter.MarkRunning(string chapterId)` in `PipelineCommand.CompactPipelineProgressReporter` is an intentional no-op with an empty method body. The implementation ignores `chapterId` and performs no state changes or output, matching the class’s null-object behavior for `IPipelineProgressReporter`.


#### [[NullProgressReporter.MarkRunning]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void MarkRunning(string chapterId)
```

