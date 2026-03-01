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
  - llm/validation
  - llm/error-handling
---
# CompactPipelineProgressReporter::MarkFailed
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Marks an existing chapter’s progress record as finished-with-failure and assigns a normalized failure message.**

In `PipelineCommand.CompactPipelineProgressReporter`, `MarkFailed` applies all state changes through `UpdateChapter(chapterId, status => ...)` rather than mutating storage directly. The updater sets `status.Stage = PipelineStage.Complete`, flips `status.IsRunning` to `false`, and marks `status.Failed = true` to represent a terminal failed outcome. It also normalizes the display message to `Failed` for null/whitespace input, otherwise formatting it as `Failed: {message}`, while `UpdateChapter` handles chapter lookup, synchronization, and refresh.


#### [[CompactPipelineProgressReporter.MarkFailed]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void MarkFailed(string chapterId, string message)
```

**Calls ->**
- [[CompactPipelineProgressReporter.UpdateChapter]]

