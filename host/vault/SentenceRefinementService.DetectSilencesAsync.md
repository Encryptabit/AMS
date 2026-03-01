---
namespace: "Ams.Core.Pipeline"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Pipeline/SentenceRefinementService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/async
  - llm/utility
  - llm/error-handling
---
# SentenceRefinementService::DetectSilencesAsync
**Path**: `Projects/AMS/host/Ams.Core/Pipeline/SentenceRefinementService.cs`

## Summary
**Runs ffmpeg silence detection for an audio file and converts its diagnostic output into structured silence intervals.**

DetectSilencesAsync resolves the ffmpeg executable from `FFMPEG_EXE` (fallback `"ffmpeg"`), then builds a `ProcessStartInfo` that runs `-af silencedetect=noise={thresholdDb}dB:duration={minDurationSec}` against the input audio with redirected stdio and no shell window. It starts the process (throwing `InvalidOperationException` if start fails), awaits `WaitForExitAsync` with the service-level `_timeout` cancellation token, reads stderr, and delegates parsing to `ParseSilenceOutput`. The method returns the parsed `SilenceInfo[]` extracted from ffmpeg’s silence detector output.


#### [[SentenceRefinementService.DetectSilencesAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private Task<SilenceInfo[]> DetectSilencesAsync(string audioPath, double thresholdDb, double minDurationSec)
```

**Calls ->**
- [[SentenceRefinementService.ParseSilenceOutput]]

**Called-by <-**
- [[SentenceRefinementService.RefineAsync]]

