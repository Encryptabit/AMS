---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseDynamicsService::BuildWordCenters
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Generates time-center anchors for non-empty words in a sentence’s book-word range.**

`BuildWordCenters` computes approximate per-word center timestamps within a sentence by linearly distributing sentence duration across the clamped `BookRange` word span in `bookIndex.Words`. It returns early for non-positive sentence duration or empty effective word span, skips empty tokens, and for each remaining word computes `(wordStart + wordEnd) * 0.5`. The resulting ordered centers are used as soft priors during silence-to-punctuation alignment.


#### [[PauseDynamicsService.BuildWordCenters]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<double> BuildWordCenters(SentenceAlign sentence, BookIndex bookIndex)
```

**Called-by <-**
- [[PauseDynamicsService.MatchSilencesToPunctuation]]

