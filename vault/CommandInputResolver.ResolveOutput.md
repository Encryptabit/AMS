---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# CommandInputResolver::ResolveOutput
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs`


#### [[CommandInputResolver.ResolveOutput]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static FileInfo ResolveOutput(FileInfo provided, string suffix)
```

**Calls ->**
- [[ReplState.ResolveChapterFile]]

**Called-by <-**
- [[DspCommand.ResolveOutputFile]]

