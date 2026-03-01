---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "public"
complexity: 6
fan_in: 3
fan_out: 0
tags:
  - method
---
# ReplState::ResolveChapterFile
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Repl/ReplContext.cs`


#### [[ReplState.ResolveChapterFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo ResolveChapterFile(string suffix, bool mustExist)
```

**Called-by <-**
- [[CommandInputResolver.ResolveChapterArtifact]]
- [[CommandInputResolver.ResolveOutput]]
- [[CommandInputResolver.TryResolveChapterArtifact]]

