---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/AlignCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 3
tags:
  - method
---
# AlignCommand::Create
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/AlignCommand.cs`


#### [[AlignCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create(ComputeAnchorsCommand anchorsCommand, BuildTranscriptIndexCommand transcriptCommand, HydrateTranscriptCommand hydrateCommand)
```

**Calls ->**
- [[AlignCommand.CreateAnchors]]
- [[AlignCommand.CreateHydrateTx]]
- [[AlignCommand.CreateTranscriptIndex]]

**Called-by <-**
- [[Program.Main]]

