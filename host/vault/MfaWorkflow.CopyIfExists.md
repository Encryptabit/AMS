---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/error-handling
---
# MfaWorkflow::CopyIfExists
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`

## Summary
**It best-effort copies a file to a target path only when the source exists, creating parent directories as needed.**

`CopyIfExists` performs guarded artifact copying by returning early when `sourcePath` does not exist. When present, it ensures the destination directory exists (`Directory.CreateDirectory` on `Path.GetDirectoryName(destinationPath)`) and copies with overwrite enabled (`File.Copy(..., overwrite: true)`). All IO failures are caught and logged via `Log.Debug`, making the operation non-fatal.


#### [[MfaWorkflow.CopyIfExists]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void CopyIfExists(string sourcePath, string destinationPath)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[MfaWorkflow.RunChapterAsync]]

