---
namespace: "Ams.Cli.Workspace"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Workspace/CliWorkspace.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 2
tags:
  - method
---
# CliWorkspace::OpenChapter
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Workspace/CliWorkspace.cs`


#### [[CliWorkspace.OpenChapter]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ChapterContextHandle OpenChapter(ChapterOpenOptions options)
```

**Calls ->**
- [[CliWorkspace.NormalizeOptions]]
- [[ChapterManager.CreateContext]]

