---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseDynamicsService::IsIntraSentencePunctuation
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Defines which punctuation marks are treated as intra-sentence pause cues.**

`IsIntraSentencePunctuation` is a single-character classifier that returns `true` only for comma (`','`). It centralizes the punctuation criterion used when detecting intra-sentence pause anchors from text/token streams.


#### [[PauseDynamicsService.IsIntraSentencePunctuation]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsIntraSentencePunctuation(char ch)
```

**Called-by <-**
- [[PauseDynamicsService.ExtractTimesFromBookWords]]
- [[PauseDynamicsService.ExtractTimesFromScript]]

