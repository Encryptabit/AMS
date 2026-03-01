---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseDynamicsService::BuildSentenceParagraphMap
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Builds a lookup that maps sentence indices to paragraph indices from word-level book metadata.**

`BuildSentenceParagraphMap` creates a sentence-to-paragraph lookup by iterating `bookIndex.Words` and collecting entries where both `SentenceIndex` and `ParagraphIndex` are non-negative. For each qualifying word it assigns `map[word.SentenceIndex] = word.ParagraphIndex`, so later words overwrite earlier mappings for the same sentence key. The result is a dictionary keyed by sentence ID with inferred paragraph IDs.


#### [[PauseDynamicsService.BuildSentenceParagraphMap]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Dictionary<int, int> BuildSentenceParagraphMap(BookIndex bookIndex)
```

**Called-by <-**
- [[PauseDynamicsService.AnalyzeChapter]]
- [[PauseDynamicsService.Execute]]

