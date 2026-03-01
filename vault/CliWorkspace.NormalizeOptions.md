---
namespace: "Ams.Cli.Workspace"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Workspace/CliWorkspace.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
---
# CliWorkspace::NormalizeOptions
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Workspace/CliWorkspace.cs`


#### [[CliWorkspace.NormalizeOptions]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private ChapterOpenOptions NormalizeOptions(ChapterOpenOptions options)
```

**Calls ->**
- [[CliWorkspace.ResolveDefaultBookIndex]]

**Called-by <-**
- [[CliWorkspace.OpenChapter]]

