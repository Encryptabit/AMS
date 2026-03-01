---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# FfSession::BuildFailureHint
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfSession.cs`

## Summary
**Generates an actionable error hint explaining where FFmpeg native binaries should be located when initialization fails.**

`BuildFailureHint` composes a diagnostic message for FFmpeg binding failures using current runtime state. It conditionally includes either a generic discovery failure note or the attempted `ffmpeg.RootPath`, then appends concrete deployment guidance about placing native libraries under `ExtTools/ffmpeg/bin` (or legacy `ExtTools/ffmpeg/binaries`) and suggested download sources. The method is pure string construction with no I/O.


#### [[FfSession.BuildFailureHint]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildFailureHint()
```

**Called-by <-**
- [[FfSession.EnsureInitialized]]

