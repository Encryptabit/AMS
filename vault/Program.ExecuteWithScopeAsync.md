---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 9
fan_in: 2
fan_out: 5
tags:
  - method
---
# Program::ExecuteWithScopeAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Program.cs`


#### [[Program.ExecuteWithScopeAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task ExecuteWithScopeAsync(string[] args, ReplState state, RootCommand rootCommand)
```

**Calls ->**
- [[Program.ExecuteChaptersInParallelAsync]]
- [[Program.ReplacePlaceholders]]
- [[Program.ShouldHandleAllChaptersInBulk]]
- [[Program.TryGetAsrParallelism]]
- [[ReplState.BeginChapterScope]]

**Called-by <-**
- [[Program.StartRepl]]
- [[Program.TryHandleBuiltInAsync]]

