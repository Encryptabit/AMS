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
  - llm/error-handling
---
# MfaDetachedProcessRunner::TryDeleteScript
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs`

## Summary
**It attempts to remove the generated temporary PowerShell script without propagating deletion errors.**

`TryDeleteScript` performs best-effort temp-script cleanup by checking `File.Exists(scriptPath)` and deleting with `File.Delete(scriptPath)`. All exceptions are swallowed in a broad `catch`, intentionally preventing cleanup failures from impacting caller flow (`RunAsync` finally block). The method is side-effect only and non-throwing by design.


#### [[MfaDetachedProcessRunner.TryDeleteScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void TryDeleteScript(string scriptPath)
```

**Called-by <-**
- [[MfaDetachedProcessRunner.RunAsync]]

