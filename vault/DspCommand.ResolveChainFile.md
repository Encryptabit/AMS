---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 6
fan_out: 0
tags:
  - method
---
# DspCommand::ResolveChainFile
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.ResolveChainFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo ResolveChainFile(FileInfo provided, string baseDirectory = null)
```

**Called-by <-**
- [[DspCommand.CreateChainAddCommand]]
- [[DspCommand.CreateChainInsertCommand]]
- [[DspCommand.CreateChainListCommand]]
- [[DspCommand.CreateChainPrependCommand]]
- [[DspCommand.CreateChainRemoveCommand]]
- [[DspCommand.CreateRunCommand]]

