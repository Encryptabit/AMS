---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 3
tags:
  - method
---
# Program::ExecuteChaptersInParallelAsync
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Program.cs`


#### [[Program.ExecuteChaptersInParallelAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task ExecuteChaptersInParallelAsync(ReplState state, RootCommand rootCommand, string[] args, int requestedParallelism)
```

**Calls ->**
- [[Program.ReplacePlaceholders]]
- [[ReplState.BeginChapterScope]]
- [[Log.Debug]]

**Called-by <-**
- [[Program.ExecuteWithScopeAsync]]

