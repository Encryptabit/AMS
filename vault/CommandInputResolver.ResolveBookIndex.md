---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs"
access_modifier: "public"
complexity: 5
fan_in: 11
fan_out: 1
tags:
  - method
  - danger/high-fan-in
---
# CommandInputResolver::ResolveBookIndex
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs`

> [!danger] High Fan-In (11)
> This method is called by 11 other methods. Changes here have wide impact.


#### [[CommandInputResolver.ResolveBookIndex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static FileInfo ResolveBookIndex(FileInfo provided, bool mustExist = true)
```

**Calls ->**
- [[ReplState.ResolveBookIndex]]

**Called-by <-**
- [[AlignCommand.CreateAnchors]]
- [[AlignCommand.CreateHydrateTx]]
- [[AlignCommand.CreateTranscriptIndex]]
- [[AsrCommand.Create]]
- [[BookCommand.CreatePopulatePhonemes]]
- [[BookCommand.CreateVerify]]
- [[BuildIndexCommand.Create]]
- [[PipelineCommand.CreateRun]]
- [[TreatCommand.Create]]
- [[ValidateCommand.CreateReportCommand]]
- [[ValidateCommand.CreateTimingCommand]]

