---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# Program::NormalizeParallelism
**Path**: `Projects/AMS/host/Ams.Cli/Program.cs`

## Summary
**Convert a raw requested parallelism argument into a safe effective concurrency setting.**

`NormalizeParallelism` is a small guard helper used by `ExtractParallelism` that applies one conditional branch (complexity 2) to sanitize the requested concurrency input. The implementation normalizes invalid/non-positive requests to a default/auto parallelism value and otherwise returns a usable positive integer, with no I/O or side effects.


#### [[Program.NormalizeParallelism]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int NormalizeParallelism(int requested)
```

**Called-by <-**
- [[Program.ExtractParallelism]]

