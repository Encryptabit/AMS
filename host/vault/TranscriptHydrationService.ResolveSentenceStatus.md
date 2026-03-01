---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptHydrationService::ResolveSentenceStatus
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`

## Summary
**Computes a sentence quality status string from alignment metrics for downstream hydration output.**

This private static classifier maps `SentenceMetrics` to a coarse quality label using fixed thresholds. It returns `"ok"` only when `metrics.Wer <= 0.10` and `metrics.MissingRuns < 3`, otherwise it falls back to `"attention"` for `Wer <= 0.25` and `"unreliable"` for higher error rates. The implementation is an expression-bodied nested ternary, with sentence-level strictness added via the `MissingRuns` gate.


#### [[TranscriptHydrationService.ResolveSentenceStatus]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveSentenceStatus(SentenceMetrics metrics)
```

**Called-by <-**
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]

