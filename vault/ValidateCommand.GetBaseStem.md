---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 5
fan_out: 1
tags:
  - method
---
# ValidateCommand::GetBaseStem
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`


#### [[ValidateCommand.GetBaseStem]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string GetBaseStem(string fileName)
```

**Calls ->**
- [[ValidateCommand.NormalizeStem]]

**Called-by <-**
- [[ValidateCommand.BuildOutputJsonPath]]
- [[ValidateCommand.ResolveAdjustmentsPath]]
- [[ValidateCommand.ResolveAudioPath]]
- [[ValidateCommand.ResolveDefaultReportPath]]
- [[ValidateCommand.TryInferChapterId]]

