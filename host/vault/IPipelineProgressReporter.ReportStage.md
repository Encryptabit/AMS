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
  - llm/async
---
# IPipelineProgressReporter::ReportStage
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Reports a chapter’s current pipeline stage and message to whichever progress reporter implementation is active.**

`IPipelineProgressReporter.ReportStage` is a progress-reporting callback invoked by `RunPipelineAsync` after each major stage (`BookIndex`, `Asr`, `Anchors`, `Transcript`, `Hydrate`, `Mfa`) with a chapter id and status message. In `PipelineProgressReporter`, it delegates to `Update`, which lock-protects task updates, clamps numeric progress to `PipelineStageCount`, and rebuilds the Spectre.Console description string. In `CompactPipelineProgressReporter`, it updates in-memory `ChapterStatus` and refreshes the live table, while `NullProgressReporter` is a no-op.


#### [[IPipelineProgressReporter.ReportStage]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void ReportStage(string chapterId, PipelineStage stage, string message)
```

**Called-by <-**
- [[PipelineCommand.RunPipelineAsync]]

