---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# ValidateCommand::BuildOutputJsonPath
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`


#### [[ValidateCommand.BuildOutputJsonPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo BuildOutputJsonPath(FileInfo reference, string suffix)
```

**Calls ->**
- [[ValidateCommand.GetBaseStem]]

**Called-by <-**
- [[ValidateCommand.TryResolveAdjustedArtifact]]

