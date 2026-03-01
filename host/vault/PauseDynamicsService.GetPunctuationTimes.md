---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseDynamicsService::GetPunctuationTimes
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Resolves punctuation timing anchors for a sentence by preferring hydrated text-derived positions and falling back to book-word-based estimates.**

`GetPunctuationTimes` derives candidate punctuation anchor times within a sentence using a prioritized fallback chain. It computes sentence start/end/duration from `SentenceAlign.Timing`, first attempts extraction from `hydratedSentence.BookText` via `ExtractTimesFromScript`, then falls back to `hydratedSentence.ScriptText` with the same routine, and finally falls back to token-based estimation from `BookIndex` words via `ExtractTimesFromBookWords`. It returns the first non-empty time list produced by that sequence.


#### [[PauseDynamicsService.GetPunctuationTimes]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<double> GetPunctuationTimes(SentenceAlign sentence, BookIndex bookIndex, HydratedSentence hydratedSentence)
```

**Calls ->**
- [[PauseDynamicsService.ExtractTimesFromBookWords]]
- [[PauseDynamicsService.ExtractTimesFromScript]]

**Called-by <-**
- [[PauseDynamicsService.BuildIntraSentenceSpans]]

