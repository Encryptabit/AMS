---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs"
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
# MfaDetachedProcessRunner::NormalizeWorkingDirectory
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs`

## Summary
**It sanitizes an optional working-directory value into an absolute path when possible, with graceful fallback on normalization errors.**

`NormalizeWorkingDirectory` returns `null` when the input is null/whitespace, otherwise it attempts to canonicalize the path with `Path.GetFullPath(directory)`. If path normalization throws (e.g., invalid path format), it catches the exception and falls back to returning the original input string. This keeps downstream process setup resilient to malformed but potentially usable directory values.


#### [[MfaDetachedProcessRunner.NormalizeWorkingDirectory]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string NormalizeWorkingDirectory(string directory)
```

**Called-by <-**
- [[MfaDetachedProcessRunner.RunAsync]]

