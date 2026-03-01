---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# Program::TryGetAsrParallelism
**Path**: `Projects/AMS/host/Ams.Cli/Program.cs`

## Summary
**Determine whether an `asr run` invocation requested parallel execution and return the validated parallelism degree via an out parameter.**

`TryGetAsrParallelism` initializes `parallelism` to `1`, then short-circuits unless the argument list has at least two tokens and starts with `asr run` using ordinal-ignore-case checks. When the command matches, it delegates option parsing to `ExtractParallelism(args)`, enforces a lower bound with `Math.Max(1, ...)`, and stores the result in the `out` parameter. It returns `true` only when the resolved value is greater than `1`, so callers (`ExecuteWithScopeAsync`) can branch into parallel chapter execution only when explicitly requested.


#### [[Program.TryGetAsrParallelism]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryGetAsrParallelism(IReadOnlyList<string> args, out int parallelism)
```

**Calls ->**
- [[Program.ExtractParallelism]]

**Called-by <-**
- [[Program.ExecuteWithScopeAsync]]

