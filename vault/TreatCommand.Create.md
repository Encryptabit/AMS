---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/TreatCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 11
tags:
  - method
---
# TreatCommand::Create
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/TreatCommand.cs`


#### [[TreatCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create()
```

**Calls ->**
- [[CommandInputResolver.ResolveBookIndex]]
- [[CommandInputResolver.ResolveWorkspace]]
- [[AudioTreatmentService.TreatChapterAsync]]
- [[AudioTreatmentService.TreatChapterAsync_2]]
- [[Log.Debug]]
- [[Log.Error_2]]
- [[Log.Error]]
- [[Log.Info]]
- [[Log.Warn]]
- [[ChapterContext.ResolveArtifactFile]]
- [[IWorkspace.OpenChapter]]

**Called-by <-**
- [[Program.Main]]

