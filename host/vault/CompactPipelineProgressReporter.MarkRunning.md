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
  - llm/utility
---
# CompactPipelineProgressReporter::MarkRunning
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Marks a chapter as running in the compact progress reporter and updates the rendered pipeline view.**

`MarkRunning` delegates mutation to `UpdateChapter(chapterId, ...)`, so the update runs under the reporter’s synchronization lock and triggers a live UI refresh. Its updater sets `status.IsRunning = true` and conditionally sets `status.Message = "In progress..."` only when `status.Stage == PipelineStage.Pending`, leaving existing stage messages intact otherwise. If the chapter ID is not present, `UpdateChapter` exits early and no state is changed.


#### [[CompactPipelineProgressReporter.MarkRunning]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void MarkRunning(string chapterId)
```

**Calls ->**
- [[CompactPipelineProgressReporter.UpdateChapter]]

