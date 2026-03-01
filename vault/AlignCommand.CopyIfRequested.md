---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/AlignCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 3
fan_out: 1
tags:
  - method
---
# AlignCommand::CopyIfRequested
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/AlignCommand.cs`


#### [[AlignCommand.CopyIfRequested]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void CopyIfRequested(FileInfo source, FileInfo destination)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[AlignCommand.CreateAnchors]]
- [[AlignCommand.CreateHydrateTx]]
- [[AlignCommand.CreateTranscriptIndex]]

