---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 11
tags:
  - method
  - llm/factory
  - llm/di
  - llm/entry-point
  - llm/async
  - llm/validation
  - llm/data-access
  - llm/error-handling
---
# ValidateCommand::CreateReportCommand
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Construct a command that resolves chapter validation inputs, generates a report asynchronously, and saves it with explicit error reporting.**

`CreateReportCommand` is a thin factory that builds the report-oriented CLI `Command` and captures the injected `ValidationService` in its handler. The handler resolves runtime context and paths through `ResolveWorkspace`, `ResolveBookIndex`, `OpenChapter`, `TryInferChapterId`, `TryResolveChapterArtifact`, and `ResolveDefaultReportPath`, then runs `BuildReportAsync` and persists output via `Save`. It uses `Debug`/`Error` logging for failure paths while keeping control flow linear (complexity 1).


#### [[ValidateCommand.CreateReportCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateReportCommand(ValidationService validationService)
```

**Calls ->**
- [[ValidateCommand.ResolveDefaultReportPath]]
- [[ValidateCommand.TryInferChapterId]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[CommandInputResolver.ResolveWorkspace]]
- [[CommandInputResolver.TryResolveChapterArtifact]]
- [[ValidationService.BuildReportAsync]]
- [[Log.Debug]]
- [[Log.Error_2]]
- [[Log.Error]]
- [[ChapterContextHandle.Save]]
- [[IWorkspace.OpenChapter]]

**Called-by <-**
- [[ValidateCommand.Create]]

