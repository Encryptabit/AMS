---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 15
fan_in: 0
fan_out: 4
tags:
  - method
  - danger/high-complexity
---
# ValidateCommand::ResolveAudioPath
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

> [!danger] High Complexity (15)
> Cyclomatic complexity: 15. Consider refactoring into smaller methods.


#### [[ValidateCommand.ResolveAudioPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveAudioPath(TranscriptIndex transcript, HydratedTranscript hydrated, FileInfo txFile, FileInfo hydrateFile)
```

**Calls ->**
- [[ValidateCommand.GetBaseStem]]
- [[ValidateCommand.MakeAbsolute]]
- [[ValidateCommand.NormalizeStem]]
- [[Register]]

