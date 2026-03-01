---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/AlignCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 10
tags:
  - method
---
# AlignCommand::CreateTranscriptIndex
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/AlignCommand.cs`


#### [[AlignCommand.CreateTranscriptIndex]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Command CreateTranscriptIndex(BuildTranscriptIndexCommand command)
```

**Calls ->**
- [[AlignCommand.CopyIfRequested]]
- [[CommandInputResolver.RequireAudio]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[CommandInputResolver.ResolveChapterArtifact]]
- [[CommandInputResolver.ResolveWorkspace]]
- [[BuildTranscriptIndexCommand.ExecuteAsync]]
- [[Log.Error]]
- [[ChapterContextHandle.Save]]
- [[ChapterDocuments.GetTranscriptFile]]
- [[IWorkspace.OpenChapter]]

**Called-by <-**
- [[AlignCommand.Create]]

