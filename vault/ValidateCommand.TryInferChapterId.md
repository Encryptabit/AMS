---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# ValidateCommand::TryInferChapterId
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`


#### [[ValidateCommand.TryInferChapterId]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string TryInferChapterId(FileInfo txFile, FileInfo hydrateFile)
```

**Calls ->**
- [[ValidateCommand.GetBaseStem]]

**Called-by <-**
- [[ValidateCommand.CreateReportCommand]]

