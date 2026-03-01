---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 9
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptHydrationService::JoinAsr
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`

## Summary
**Produces normalized ASR-side text for a nullable inclusive word-index range.**

`JoinAsr` builds a normalized ASR text span from an optional inclusive index range. It returns `string.Empty` when `start/end` are missing or out of bounds (`s < 0`, `e >= asr.WordCount`, `e < s`), otherwise iterates `i = s..e`, collects non-empty tokens from `asr.GetWord(i)`, joins them with spaces, and normalizes the result via `NormalizeSurface`. Empty collections also collapse to an empty normalized string.


#### [[TranscriptHydrationService.JoinAsr]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string JoinAsr(AsrResponse asr, int? start, int? end)
```

**Calls ->**
- [[AsrResponse.GetWord]]
- [[TranscriptHydrationService.NormalizeSurface]]

**Called-by <-**
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]

