---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# CompactPipelineProgressReporter::RunAsync
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Execute the pipeline asynchronously with live compact progress rendering, while falling back to a no-op progress reporter when there are no chapters.**

`RunAsync` short-circuits to `run(new NullProgressReporter())` when `chapters` is `null` or empty. Otherwise, it creates a `CompactPipelineProgressReporter`, builds the initial Spectre.Console table via `BuildView()`, and runs the delegate inside `AnsiConsole.Live(initial).AutoClear(false).Overflow(Ellipsis).Cropping(Top).StartAsync(...)`. Inside the live callback it calls `Attach(ctx)`, awaits `run(reporter)` with `ConfigureAwait(false)`, and guarantees `MarkFinished()` in `finally` so final render/runtime state is updated even on failure.


#### [[CompactPipelineProgressReporter.RunAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task RunAsync(IReadOnlyList<FileInfo> chapters, Func<PipelineCommand.IPipelineProgressReporter, Task> run)
```

**Calls ->**
- [[CompactPipelineProgressReporter.Attach]]
- [[CompactPipelineProgressReporter.BuildView]]
- [[CompactPipelineProgressReporter.MarkFinished]]

**Called-by <-**
- [[PipelineCommand.CreateRun]]

