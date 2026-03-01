---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 5
tags:
  - method
---
# ValidateCommand::CreateTimingInitCommand
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`


#### [[ValidateCommand.CreateTimingInitCommand]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateTimingInitCommand()
```

**Calls ->**
- [[Log.Debug]]
- [[Log.Error_2]]
- [[Log.Error]]
- [[PausePolicyPresets.House]]
- [[PausePolicyStorage.Save]]

**Called-by <-**
- [[ValidateCommand.CreateTimingCommand]]

