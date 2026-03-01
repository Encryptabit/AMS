---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/AlignCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 9
tags:
  - method
---
# AlignCommand::CreateHydrateTx
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/AlignCommand.cs`


#### [[AlignCommand.CreateHydrateTx]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateHydrateTx(HydrateTranscriptCommand command)
```

**Calls ->**
- [[AlignCommand.CopyIfRequested]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[CommandInputResolver.ResolveChapterArtifact]]
- [[CommandInputResolver.ResolveWorkspace]]
- [[HydrateTranscriptCommand.ExecuteAsync]]
- [[Log.Error]]
- [[ChapterContextHandle.Save]]
- [[ChapterDocuments.GetHydratedTranscriptFile]]
- [[IWorkspace.OpenChapter]]

**Called-by <-**
- [[AlignCommand.Create]]

