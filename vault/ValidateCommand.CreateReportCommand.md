---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 11
tags:
  - method
---
# ValidateCommand::CreateReportCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`


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

