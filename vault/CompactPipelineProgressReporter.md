---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
base_class: ~
interfaces:
  - "Ams.Cli.Commands.PipelineCommand.IPipelineProgressReporter"
member_count: 14
dependency_count: 0
pattern: ~
tags:
  - class
---

# CompactPipelineProgressReporter

> Class in `Ams.Cli.Commands`

**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

**Implements**:
- [[IPipelineProgressReporter]]

## Properties
- `_sync`: object
- `_chapters`: Dictionary<string, ChapterStatus>
- `_chapterOrder`: List<string>
- `_stopwatch`: Stopwatch
- `_liveContext`: LiveDisplayContext?
- `_finished`: bool

## Members
- [[CompactPipelineProgressReporter..ctor]]
- [[CompactPipelineProgressReporter.RunAsync]]
- [[CompactPipelineProgressReporter.Attach]]
- [[CompactPipelineProgressReporter.SetQueued]]
- [[CompactPipelineProgressReporter.MarkRunning]]
- [[CompactPipelineProgressReporter.ReportStage]]
- [[CompactPipelineProgressReporter.MarkComplete]]
- [[CompactPipelineProgressReporter.MarkFailed]]
- [[CompactPipelineProgressReporter.UpdateChapter]]
- [[CompactPipelineProgressReporter.RefreshUnsafe]]
- [[CompactPipelineProgressReporter.MarkFinished]]
- [[CompactPipelineProgressReporter.BuildView]]
- [[CompactPipelineProgressReporter.BuildStageMarkup]]
- [[CompactPipelineProgressReporter.FormatElapsed]]

