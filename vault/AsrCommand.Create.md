---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/AsrCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 10
tags:
  - method
---
# AsrCommand::Create
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/AsrCommand.cs`


#### [[AsrCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create(GenerateTranscriptCommand transcriptCommand)
```

**Calls ->**
- [[CommandInputResolver.RequireAudio]]
- [[CommandInputResolver.ResolveBookIndex]]
- [[CommandInputResolver.ResolveWorkspace]]
- [[GenerateTranscriptCommand.ExecuteAsync]]
- [[AsrEngineConfig.Resolve]]
- [[Log.Debug]]
- [[Log.Error]]
- [[ChapterContextHandle.Save]]
- [[ChapterDocuments.GetAsrFile]]
- [[IWorkspace.OpenChapter]]

**Called-by <-**
- [[Program.Main]]

