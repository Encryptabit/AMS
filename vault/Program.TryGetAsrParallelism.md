---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# Program::TryGetAsrParallelism
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Program.cs`


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

