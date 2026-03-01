---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# SentenceCollector::ResolveTiming
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Selects the effective sentence timing, preferring valid hydrated timing over aligned sentence timing.**

`ResolveTiming` chooses the source of sentence timing with a hydrated-first fallback strategy. If `hydrated?.Timing` exists and has positive duration, it returns `new SentenceTiming(timing)` from hydrated data; otherwise it falls back to `new SentenceTiming(sentence.Timing)` from alignment data. This ensures collector timing prefers enriched hydrated timing when valid.


#### [[SentenceCollector.ResolveTiming]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static SentenceTiming ResolveTiming(SentenceAlign sentence, HydratedSentence hydrated)
```

**Called-by <-**
- [[SentenceCollector..ctor]]

