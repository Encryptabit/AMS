---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
---
# MfaDetachedProcessRunner::WriteScript
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaDetachedProcessRunner.cs`

## Summary
**It materializes the detached MFA execution script on disk and returns its temp-file location.**

`WriteScript` generates a unique temporary PowerShell script path under `Path.GetTempPath()` using a GUID (`ams-mfa-detached-{N}.ps1`), builds script content via `BuildScript(command, workingDirectory)`, and writes it with `File.WriteAllText(..., Encoding.UTF8)`. It returns the script file path for later process execution and cleanup.


#### [[MfaDetachedProcessRunner.WriteScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string WriteScript(string command, string workingDirectory)
```

**Calls ->**
- [[MfaDetachedProcessRunner.BuildScript]]

**Called-by <-**
- [[MfaDetachedProcessRunner.RunAsync]]

