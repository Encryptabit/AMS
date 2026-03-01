---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs"
access_modifier: "public"
complexity: 6
fan_in: 3
fan_out: 1
tags:
  - method
---
# CommandInputResolver::TryResolveChapterArtifact
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs`


#### [[CommandInputResolver.TryResolveChapterArtifact]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static FileInfo TryResolveChapterArtifact(FileInfo provided, string suffix, bool mustExist = true)
```

**Calls ->**
- [[ReplState.ResolveChapterFile]]

**Called-by <-**
- [[DspCommand.ResolveFilteredOutput]]
- [[ValidateCommand.CreateReportCommand]]
- [[ValidateCommand.CreateTimingCommand]]

