---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/RefineSentencesCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
---
# RefineSentencesCommand::RunAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/RefineSentencesCommand.cs`


#### [[RefineSentencesCommand.RunAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task RunAsync(FileInfo txFile, FileInfo asrFile, FileInfo audioFile, FileInfo outFile, string language, bool withSilence, double silenceDb, double silenceMin)
```

**Calls ->**
- [[Log.Debug]]
- [[SentenceRefinementService.RefineAsync]]

**Called-by <-**
- [[RefineSentencesCommand.Create]]

