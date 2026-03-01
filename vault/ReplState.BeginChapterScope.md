---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "public"
complexity: 1
fan_in: 2
fan_out: 0
tags:
  - method
---
# ReplState::BeginChapterScope
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Repl/ReplContext.cs`


#### [[ReplState.BeginChapterScope]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IDisposable BeginChapterScope(FileInfo chapter)
```

**Called-by <-**
- [[Program.ExecuteChaptersInParallelAsync]]
- [[Program.ExecuteWithScopeAsync]]

