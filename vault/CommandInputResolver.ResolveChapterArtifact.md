---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs"
access_modifier: "public"
complexity: 3
fan_in: 4
fan_out: 1
tags:
  - method
---
# CommandInputResolver::ResolveChapterArtifact
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs`


#### [[CommandInputResolver.ResolveChapterArtifact]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static FileInfo ResolveChapterArtifact(FileInfo provided, string suffix, bool mustExist = true)
```

**Calls ->**
- [[ReplState.ResolveChapterFile]]

**Called-by <-**
- [[AlignCommand.CreateAnchors]]
- [[AlignCommand.CreateHydrateTx]]
- [[AlignCommand.CreateTranscriptIndex]]
- [[RefineSentencesCommand.Create]]

