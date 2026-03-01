---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 5
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# CompactPipelineProgressReporter::UpdateChapter
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Atomically update a known chapter’s in-memory progress state and refresh the live progress view.**

`UpdateChapter` is a private synchronization helper that centralizes all chapter-state mutations in `CompactPipelineProgressReporter`. It first performs a dictionary lookup (`_chapters.TryGetValue(chapterId, out var status)`) and exits early for unknown chapter IDs, then takes `_sync`, applies the caller-supplied `Action<ChapterStatus>` to the existing status object, and invokes `RefreshUnsafe()` to rebuild/update the live Spectre display. This is the common mutation path used by `SetQueued`, `MarkRunning`, `ReportStage`, `MarkComplete`, and `MarkFailed`, ensuring thread-safe updates with immediate UI refresh.


#### [[CompactPipelineProgressReporter.UpdateChapter]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void UpdateChapter(string chapterId, Action<PipelineCommand.CompactPipelineProgressReporter.ChapterStatus> updater)
```

**Calls ->**
- [[CompactPipelineProgressReporter.RefreshUnsafe]]

**Called-by <-**
- [[CompactPipelineProgressReporter.MarkComplete]]
- [[CompactPipelineProgressReporter.MarkFailed]]
- [[CompactPipelineProgressReporter.MarkRunning]]
- [[CompactPipelineProgressReporter.ReportStage]]
- [[CompactPipelineProgressReporter.SetQueued]]

