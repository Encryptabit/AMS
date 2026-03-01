---
namespace: "Ams.Core.Application.Processes"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# MfaProcessSupervisor::NormalizeWorkingDirectory
**Path**: `Projects/AMS/host/Ams.Core/Application/Processes/MfaProcessSupervisor.cs`

## Summary
**Convert an optional working-directory value to a normalized absolute path when possible, while preserving input on resolution failure.**

`NormalizeWorkingDirectory` canonicalizes the optional working-directory input before command dispatch. It returns `null` for null/empty/whitespace values, otherwise attempts `Path.GetFullPath(workingDirectory)` to normalize relative paths. If full-path resolution throws, it falls back to the original input string instead of failing.


#### [[MfaProcessSupervisor.NormalizeWorkingDirectory]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeWorkingDirectory(string workingDirectory)
```

**Called-by <-**
- [[MfaProcessSupervisor.RunAsync]]

