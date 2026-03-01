---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# Program::ShouldHandleAllChaptersInBulk
**Path**: `Projects/AMS/host/Ams.Cli/Program.cs`

## Summary
**Determines whether the CLI args correspond to the bulk-handled `pipeline run` command.**

This helper is a strict command-shape predicate used by `ExecuteWithScopeAsync` to decide whether all chapters should be processed in one bulk invocation. It short-circuits to `false` when fewer than two arguments are provided, then requires `args[0] == "pipeline"` and `args[1] == "run"` with `StringComparison.OrdinalIgnoreCase`. Any other token values or ordering returns `false`, so only the exact `pipeline run` form enables the bulk branch.


#### [[Program.ShouldHandleAllChaptersInBulk]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool ShouldHandleAllChaptersInBulk(IReadOnlyList<string> args)
```

**Called-by <-**
- [[Program.ExecuteWithScopeAsync]]

