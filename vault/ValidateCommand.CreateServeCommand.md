---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 4
tags:
  - method
---
# ValidateCommand::CreateServeCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`


#### [[ValidateCommand.CreateServeCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateServeCommand()
```

**Calls ->**
- [[ValidateCommand.ResolveValidationViewerScript]]
- [[CommandInputResolver.ResolveDirectory]]
- [[Log.Debug]]
- [[Log.Error_2]]

**Called-by <-**
- [[ValidateCommand.Create]]

