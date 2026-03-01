---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptIndexService::ComputeTiming
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`

## Summary
**Converts an aligned script token range into concrete audio time bounds using ASR token timing metadata.**

This helper derives a `TimingRange` for a script span from ASR token timings, with defensive fallbacks. It returns `TimingRange.Empty` when word timings are unavailable or when `scriptRange` lacks valid `Start`/`End` values, then clamps both indices into `[0, asr.Tokens.Length - 1]` (and enforces `end >= start`). It computes start time from the first token’s `StartTime` and end time from the last token’s `StartTime + Duration`, then returns a new `TimingRange`.


#### [[TranscriptIndexService.ComputeTiming]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static TimingRange ComputeTiming(ScriptRange scriptRange, AsrResponse asr)
```

**Called-by <-**
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]

