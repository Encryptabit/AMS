---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseMapBuilder::BuildSentenceToParagraphMap
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Builds a mapping from hydrated sentence IDs to their containing paragraph IDs.**

`BuildSentenceToParagraphMap` creates a sentence→paragraph lookup by iterating `hydrated.Paragraphs` and assigning each `sentenceId` in `paragraph.SentenceIds` to `paragraph.Id`. It writes into a dictionary keyed by sentence ID, so later assignments overwrite earlier ones if duplicates exist.


#### [[PauseMapBuilder.BuildSentenceToParagraphMap]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Dictionary<int, int> BuildSentenceToParagraphMap(HydratedTranscript hydrated)
```

**Called-by <-**
- [[PauseMapBuilder.Build]]

