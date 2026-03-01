---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::BuildBaselineTimeline
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Create a baseline sentence-timing dictionary from a chapter pause map for use during timing-session validation.**

BuildBaselineTimeline is a private static helper called by InteractiveState’s constructor to precompute a baseline lookup of SentenceTiming values keyed by int for a chapter. Based on its signature and naming, the implementation likely traverses ChapterPauseMap data once and materializes a Dictionary<int, SentenceTiming> used by later validation logic for fast timing comparisons. With complexity 3, its flow is probably straightforward (iteration plus a small conditional branch for missing or conflicting timing data).


#### [[InteractiveState.BuildBaselineTimeline]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Dictionary<int, SentenceTiming> BuildBaselineTimeline(ChapterPauseMap chapter)
```

**Called-by <-**
- [[InteractiveState..ctor]]

