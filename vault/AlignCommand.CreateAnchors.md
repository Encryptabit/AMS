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
# AlignCommand::CreateAnchors
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/AlignCommand.cs`


#### [[AlignCommand.CreateAnchors]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateAnchors(ComputeAnchorsCommand command)
```

**Calls ->**
- [[AlignCommand.CopyIfRequested]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[CommandInputResolver.ResolveChapterArtifact]]
- [[CommandInputResolver.ResolveWorkspace]]
- [[ComputeAnchorsCommand.ExecuteAsync]]
- [[Log.Error]]
- [[ChapterContextHandle.Save]]
- [[ChapterDocuments.GetAnchorsFile]]
- [[IWorkspace.OpenChapter]]

**Called-by <-**
- [[AlignCommand.Create]]

