---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/error-handling
---
# MfaWorkflow::FindOovListFile
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`

## Summary
**It locates the most recent MFA-generated OOV list file under a directory tree with safe fallback on IO errors.**

`FindOovListFile` probes `directory` recursively for files matching `oovs_found*.txt`, orders matches by `File.GetLastWriteTimeUtc` descending, and returns the newest path via `FirstOrDefault()`. The filesystem scan is wrapped in `try/catch`; on failure it logs a debug message (`"Failed to probe for OOV list: {Message}"`) and returns `null`.


#### [[MfaWorkflow.FindOovListFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FindOovListFile(string directory)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[MfaWorkflow.RunChapterAsync]]

