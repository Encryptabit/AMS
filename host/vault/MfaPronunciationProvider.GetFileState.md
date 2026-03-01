---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# MfaPronunciationProvider::GetFileState
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`

## Summary
**It safely reports whether a file exists and, if so, its byte length.**

`GetFileState` probes file presence and size defensively by creating a `FileInfo`, calling `Refresh()`, and returning `(true, info.Length)` when the file exists, otherwise `(false, 0L)`. Any filesystem exception is swallowed and normalized to `(false, 0L)`, preventing polling callers from failing on transient IO/path issues.


#### [[MfaPronunciationProvider.GetFileState]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (bool Exists, long SizeBytes) GetFileState(string path)
```

**Called-by <-**
- [[MfaPronunciationProvider.RunG2pWithProgressAsync]]

