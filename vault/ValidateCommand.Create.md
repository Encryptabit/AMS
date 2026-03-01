---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 3
tags:
  - method
---
# ValidateCommand::Create
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`


#### [[ValidateCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create(ValidationService validationService)
```

**Calls ->**
- [[ValidateCommand.CreateReportCommand]]
- [[ValidateCommand.CreateServeCommand]]
- [[ValidateCommand.CreateTimingCommand]]

**Called-by <-**
- [[Program.Main]]

