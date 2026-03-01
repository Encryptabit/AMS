---
namespace: "Ams.Cli.Services"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Services/PlugalyzerService.cs"
access_modifier: "internal"
complexity: 3
fan_in: 3
fan_out: 2
tags:
  - method
  - llm/async
  - llm/utility
  - llm/error-handling
---
# PlugalyzerService::RunAsync
**Path**: `Projects/AMS/host/Ams.Cli/Services/PlugalyzerService.cs`

## Summary
**Execute the Plugalyzer CLI asynchronously with caller-provided arguments, working directory, cancellation, and output/error handlers, returning its exit code.**

`RunAsync` is an internal static orchestration method that resolves the target executable through `ResolveExecutable`, logs execution context via `Debug`, and runs the command with the provided argument list in `workingDirectory`. It is cancellation-aware (`CancellationToken`) and streams process output/error to optional `Action<string>` callbacks, then completes with the process exit code as `Task<int>`. With complexity 3 and only two direct helper calls, it is intentionally thin and serves as the shared execution path for `CreateInitCommand`, `CreateListParamsCommand`, and `RunChainAsync`.


#### [[PlugalyzerService.RunAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static Task<int> RunAsync(IReadOnlyList<string> arguments, string workingDirectory, CancellationToken cancellationToken, Action<string> onStdOut = null, Action<string> onStdErr = null)
```

**Calls ->**
- [[PlugalyzerService.ResolveExecutable]]
- [[Log.Debug]]

**Called-by <-**
- [[DspCommand.CreateInitCommand]]
- [[DspCommand.CreateListParamsCommand]]
- [[DspCommand.RunChainAsync]]

