---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 1
tags:
  - method
---
# Program::ExtractParallelism
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Program.cs`


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

