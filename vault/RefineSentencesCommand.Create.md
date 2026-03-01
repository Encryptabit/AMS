---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/RefineSentencesCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 4
tags:
  - method
---
# RefineSentencesCommand::Create
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/RefineSentencesCommand.cs`


#### [[RefineSentencesCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create()
```

**Calls ->**
- [[RefineSentencesCommand.RunAsync]]
- [[CommandInputResolver.RequireAudio]]
- [[CommandInputResolver.ResolveChapterArtifact]]
- [[Log.Error]]

**Called-by <-**
- [[Program.Main]]

