---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/error-handling
---
# MfaWorkflow::TryDeleteDirectory
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`

## Summary
**It attempts to delete a directory tree without failing the caller when deletion errors occur.**

`TryDeleteDirectory` performs best-effort recursive directory cleanup: it checks `Directory.Exists(path)` and calls `Directory.Delete(path, recursive: true)` when present. Any exception is caught and logged via `Log.Debug("Failed to delete stale MFA directory {Path}: {Message}", ...)`, preventing cleanup errors from propagating.


#### [[MfaWorkflow.TryDeleteDirectory]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void TryDeleteDirectory(string path)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[MfaWorkflow.CleanupMfaArtifacts]]

