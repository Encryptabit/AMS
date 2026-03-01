---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# PauseMapBuilder::BuildParagraphSentenceOrder
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Builds a paragraph-indexed sentence ordering map from hydrated transcript paragraphs.**

`BuildParagraphSentenceOrder` projects hydrated paragraph structure into a dictionary keyed by paragraph ID, with each value being a copied sentence-ID list (`paragraph.SentenceIds.ToList()`) exposed as `IReadOnlyList<int>`. It is implemented as a single LINQ `ToDictionary` transformation and preserves the sentence ordering present in each paragraph.


#### [[PauseMapBuilder.BuildParagraphSentenceOrder]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Dictionary<int, IReadOnlyList<int>> BuildParagraphSentenceOrder(HydratedTranscript hydrated)
```

**Called-by <-**
- [[PauseMapBuilder.Build]]

