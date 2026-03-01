---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 5
fan_out: 1
tags:
  - method
---
# CompactPipelineProgressReporter::UpdateChapter
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


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

