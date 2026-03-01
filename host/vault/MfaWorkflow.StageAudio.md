---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
---
# MfaWorkflow::StageAudio
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`

## Summary
**It stages audio for MFA by copying the source file only when the staged copy is absent or stale.**

`StageAudio` performs conditional copying of the chapter audio into the MFA corpus location. It copies when the destination is missing or older than `source` (based on UTC write timestamps), using `File.Copy(..., overwrite: true)`, then aligns destination metadata via `File.SetLastWriteTimeUtc(destination, source.LastWriteTimeUtc)`. If the destination is already current, it does nothing.


#### [[MfaWorkflow.StageAudio]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void StageAudio(FileInfo source, string destination)
```

**Called-by <-**
- [[MfaWorkflow.RunChapterAsync]]

