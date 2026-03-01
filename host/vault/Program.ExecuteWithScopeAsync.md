---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 9
fan_in: 2
fan_out: 5
tags:
  - method
  - llm/async
  - llm/utility
---
# Program::ExecuteWithScopeAsync
**Path**: `Projects/AMS/host/Ams.Cli/Program.cs`

## Summary
**Run a root command within the current REPL/chapter scope while resolving placeholders and coordinating optional parallel chapter execution.**

`ExecuteWithScopeAsync` is a private async orchestration method that centralizes command execution flow for both `StartRepl` and `TryHandleBuiltInAsync`. It establishes chapter-level execution context via `BeginChapterScope`, rewrites dynamic inputs with `ReplacePlaceholders`, and branches on `ShouldHandleAllChaptersInBulk` to decide bulk vs targeted execution behavior. It then delegates concurrent chapter work to `ExecuteChaptersInParallelAsync`, using `TryGetAsrParallelism` to derive runtime parallelism settings.


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

