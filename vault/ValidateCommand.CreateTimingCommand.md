---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 9
tags:
  - method
---
# ValidateCommand::CreateTimingCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`


#### [[ValidateCommand.CreateTimingCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateTimingCommand()
```

**Calls ->**
- [[ValidateCommand.CreateTimingInitCommand]]
- [[ValidateCommand.TryResolveAdjustedArtifact]]
- [[ValidateTimingSession.RunAsync]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[CommandInputResolver.ResolveWorkspace]]
- [[CommandInputResolver.TryResolveChapterArtifact]]
- [[Log.Debug]]
- [[Log.Error_2]]
- [[Log.Error]]

**Called-by <-**
- [[ValidateCommand.Create]]

