---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# AsrProcessSupervisor::TryFindOnPath
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/AsrProcessSupervisor.cs`

## Summary
**It locates the first executable/file matching `fileName` on the current PATH.**

`TryFindOnPath` performs a linear PATH search for `fileName` by splitting the `PATH` environment variable on `Path.PathSeparator`, trimming segments, and probing each `<segment>/<fileName>` with `File.Exists`. It returns the first matching absolute candidate path found, or `null` when `PATH` is empty/missing or no match exists. Blank PATH segments are skipped.


#### [[AsrProcessSupervisor.TryFindOnPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string TryFindOnPath(string fileName)
```

**Called-by <-**
- [[AsrProcessSupervisor.ResolvePowerShell]]

