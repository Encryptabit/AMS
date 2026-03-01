---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# ValidateCommand::ResolveDefaultReportPath
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`


#### [[ValidateCommand.ResolveDefaultReportPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo ResolveDefaultReportPath(FileInfo txFile, FileInfo hydrateFile)
```

**Calls ->**
- [[ValidateCommand.GetBaseStem]]

**Called-by <-**
- [[ValidateCommand.CreateReportCommand]]

