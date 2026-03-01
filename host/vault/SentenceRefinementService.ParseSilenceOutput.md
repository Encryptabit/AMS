---
namespace: "Ams.Core.Pipeline"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Pipeline/SentenceRefinementService.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# SentenceRefinementService::ParseSilenceOutput
**Path**: `Projects/AMS/host/Ams.Core/Pipeline/SentenceRefinementService.cs`

## Summary
**Converts raw ffmpeg silencedetect log text into structured silence segments with start, end, duration, and fixed confidence.**

ParseSilenceOutput performs a single-pass parse over ffmpeg stderr by splitting on `'\n'` and detecting `silence_start:` / `silence_end:` markers. It uses regex extraction (`@"silence_start:\s*([\d.]+)"`, `@"silence_end:\s*([\d.]+)"`) and `double.Parse` to capture timestamps, holding the current start in a nullable local until a matching end is found. For each completed pair, it creates `SilenceInfo(start, end, end - start, 1.0)`, resets state, and finally returns the collected intervals as an array.


#### [[SentenceRefinementService.ParseSilenceOutput]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private SilenceInfo[] ParseSilenceOutput(string ffmpegOutput)
```

**Called-by <-**
- [[SentenceRefinementService.DetectSilencesAsync]]

