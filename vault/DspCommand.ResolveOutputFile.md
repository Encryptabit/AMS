---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 2
tags:
  - method
---
# DspCommand::ResolveOutputFile
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.ResolveOutputFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo ResolveOutputFile(FileInfo provided, FileInfo inputFile)
```

**Calls ->**
- [[CommandInputResolver.ResolveOutput]]
- [[Log.Debug]]

**Called-by <-**
- [[DspCommand.CreateRunCommand]]

