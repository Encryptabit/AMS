---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/async
  - llm/utility
---
# Program::ExecuteChaptersInParallelAsync
**Path**: `Projects/AMS/host/Ams.Cli/Program.cs`

## Summary
**Runs chapter commands concurrently with bounded parallelism, creating scoped state and resolved arguments for each chapter invocation.**

`ExecuteChaptersInParallelAsync` schedules chapter-level command executions from `args` against the same `RootCommand`, using `requestedParallelism` as a concurrency cap. Each chapter run is wrapped in a per-chapter scope via `BeginChapterScope`, its argument template is resolved through `ReplacePlaceholders`, and diagnostic output is emitted through `Debug` before execution. It returns an aggregate `Task` representing completion of all chapter executions and is used by `ExecuteWithScopeAsync` for the parallel execution path.


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

