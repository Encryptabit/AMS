---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs"
access_modifier: "public"
complexity: 5
fan_in: 8
fan_out: 0
tags:
  - method
---
# CommandInputResolver::ResolveWorkspace
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs`


#### [[CommandInputResolver.ResolveWorkspace]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IWorkspace ResolveWorkspace(FileInfo bookIndexFile = null)
```

**Called-by <-**
- [[AlignCommand.CreateAnchors]]
- [[AlignCommand.CreateHydrateTx]]
- [[AlignCommand.CreateTranscriptIndex]]
- [[AsrCommand.Create]]
- [[PipelineCommand.RunPipelineAsync]]
- [[TreatCommand.Create]]
- [[ValidateCommand.CreateReportCommand]]
- [[ValidateCommand.CreateTimingCommand]]

