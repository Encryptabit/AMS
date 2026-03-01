---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/di
---
# IPipelineProgressReporter::MarkComplete
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Signals successful completion of a chapter so the configured progress reporter can finalize its status.**

`IPipelineProgressReporter.MarkComplete(string chapterId)` is a contract method on `PipelineCommand`’s nested reporter interface, so it contains no internal logic itself. `RunPipelineForMultipleChaptersAsync` invokes it right after a chapter’s `RunPipelineAsync` call returns successfully, decoupling completion signaling from execution flow. Concrete reporters (`PipelineProgressReporter`, `CompactPipelineProgressReporter`, `NullProgressReporter`) implement this by marking `PipelineStage.Complete`, updating the completion message/display state, and in the full reporter stopping the chapter task (or no-op in the null reporter).


#### [[IPipelineProgressReporter.MarkComplete]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void MarkComplete(string chapterId)
```

**Called-by <-**
- [[PipelineCommand.RunPipelineForMultipleChaptersAsync]]

