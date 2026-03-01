---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/factory
  - llm/di
---
# PipelineProgressReporter::.ctor
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Sets up and pre-populates pipeline progress tracking tasks for all chapter files in a queued initial state.**

The constructor initializes `_tasks` as a case-insensitive `Dictionary<string, ProgressTask>` and iterates the provided `chapters` list to create one progress task per file. Each key is the chapter ID derived from `Path.GetFileNameWithoutExtension(chapterFile.Name)`, and each task is created with `context.AddTask(chapterId, autoStart: true, maxValue: PipelineStageCount)`, then explicitly set to `Value = 0`. It seeds the initial UI text by calling `BuildDescription(chapterId, PipelineStage.Pending, "Queued")` before storing the task in `_tasks`.


#### [[PipelineProgressReporter..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PipelineProgressReporter(ProgressContext context, IReadOnlyList<FileInfo> chapters)
```

**Calls ->**
- [[PipelineProgressReporter.BuildDescription]]

