---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptHydrationService::ResolveParagraphStatus
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`

## Summary
**Maps a paragraph word-error-rate value to a normalized status label used in hydrated transcript output.**

This private static helper classifies paragraph quality directly from WER using fixed breakpoints in an expression-bodied nested ternary. It returns `"ok"` for `wer <= 0.10`, `"attention"` for `wer <= 0.25`, and `"unreliable"` otherwise. Unlike sentence status, it does not incorporate missing-run or other secondary metrics.


#### [[TranscriptHydrationService.ResolveParagraphStatus]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveParagraphStatus(double wer)
```

**Called-by <-**
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]

