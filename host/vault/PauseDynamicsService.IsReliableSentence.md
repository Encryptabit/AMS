---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseDynamicsService::IsReliableSentence
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Determines whether a sentence is eligible for intra-sentence pause processing based on its alignment status.**

`IsReliableSentence` is a status predicate that returns `true` only when `sentence.Status` equals `"ok"` using `StringComparison.OrdinalIgnoreCase`. It centralizes the reliability gate used before intra-sentence pause extraction.


#### [[PauseDynamicsService.IsReliableSentence]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsReliableSentence(SentenceAlign sentence)
```

**Called-by <-**
- [[PauseDynamicsService.BuildIntraSentenceSpans]]

