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
  - llm/data-access
---
# CompactPipelineProgressReporter::MarkComplete
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Marks a chapter as completed in `CompactPipelineProgressReporter` by updating its tracked status fields.**

`MarkComplete` is a thin wrapper around `UpdateChapter` that passes a status-mutating lambda for the given `chapterId`. The lambda sets `Stage` to `PipelineStage.Complete`, sets `Message` to `"Complete"`, and clears active/error flags by setting `IsRunning = false` and `Failed = false`. Missing IDs and synchronization/refresh behavior are handled inside `UpdateChapter`, not in this method.


#### [[CompactPipelineProgressReporter.MarkComplete]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void MarkComplete(string chapterId)
```

**Calls ->**
- [[CompactPipelineProgressReporter.UpdateChapter]]

