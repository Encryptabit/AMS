---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
---
# DspCommand::ResolveFilteredOutput
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/DspCommand.cs`


#### [[DspCommand.ResolveFilteredOutput]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo ResolveFilteredOutput(FileInfo explicitOutput, FileInfo inputFile)
```

**Calls ->**
- [[CommandInputResolver.TryResolveChapterArtifact]]

**Called-by <-**
- [[DspCommand.ExecuteFilterChain]]

