---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/BookCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookCommand::Median
**Path**: `Projects/AMS/host/Ams.Cli/Commands/BookCommand.cs`

## Summary
**Compute a numeric median from integer samples used by `RunVerifyAsync` verification heuristics.**

The method returns 0 for an empty list, otherwise sorts the incoming `List<int>` in place with `values.Sort()`, computes `mid = values.Count / 2`, and returns either `values[mid]` (odd length) or `(values[mid - 1] + values[mid]) / 2.0` (even length) as a `double`. This gives deterministic median calculation for `RunVerifyAsync` but intentionally mutates input ordering as part of the implementation.


#### [[BookCommand.Median]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double Median(List<int> values)
```

**Called-by <-**
- [[BookCommand.RunVerifyAsync]]

