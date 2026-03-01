---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# PauseMapBuilder::CreateParagraphCollectors
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Initializes paragraph-level collectors from paragraph-to-sentence ordering data.**

`CreateParagraphCollectors` materializes a collector map from paragraph ordering metadata by iterating `paragraphSentenceOrder` and creating `new ParagraphCollector(paragraphId, sentenceIds)` for each entry. It stores each collector in a dictionary keyed by paragraph ID and returns the completed map for later pause aggregation.


#### [[PauseMapBuilder.CreateParagraphCollectors]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Dictionary<int, PauseMapBuilder.ParagraphCollector> CreateParagraphCollectors(IReadOnlyDictionary<int, IReadOnlyList<int>> paragraphSentenceOrder)
```

**Called-by <-**
- [[PauseMapBuilder.Build]]

