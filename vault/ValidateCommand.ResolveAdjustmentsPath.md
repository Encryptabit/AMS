---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 0
fan_out: 1
tags:
  - method
---
# ValidateCommand::ResolveAdjustmentsPath
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`


#### [[ValidateCommand.ResolveAdjustmentsPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo ResolveAdjustmentsPath(FileInfo txFile, FileInfo overrideFile)
```

**Calls ->**
- [[ValidateCommand.GetBaseStem]]

