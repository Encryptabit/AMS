---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# Program::ExtractParallelism
**Path**: `Projects/AMS/host/Ams.Cli/Program.cs`

## Summary
**Parse CLI arguments to derive and normalize the effective ASR parallelism setting as an integer.**

`ExtractParallelism` is a private static helper in `Ams.Cli.Program` that inspects an `IReadOnlyList<string>` argument list and resolves a numeric parallelism value. Its branch-heavy control flow (complexity 8) indicates multiple parse/validation paths for candidate values before returning an `int`. Final coercion is delegated to `NormalizeParallelism`, and the returned value is consumed by `TryGetAsrParallelism`.


#### [[Program.ExtractParallelism]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int ExtractParallelism(IReadOnlyList<string> args)
```

**Calls ->**
- [[Program.NormalizeParallelism]]

**Called-by <-**
- [[Program.TryGetAsrParallelism]]

