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
# PauseMapBuilder::CreateSentenceCollectors
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Initializes sentence-level pause collectors for all transcript sentences with paragraph and hydration context.**

`CreateSentenceCollectors` constructs one `SentenceCollector` per transcript sentence, keyed by sentence ID. For each sentence it resolves paragraph membership from `sentenceToParagraph` (default `-1` when absent), looks up optional hydrated sentence data, and instantiates `new SentenceCollector(sentence, hydrated, paragraphId, bookIndex)`. The resulting dictionary is returned for downstream pause aggregation.


#### [[PauseMapBuilder.CreateSentenceCollectors]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Dictionary<int, PauseMapBuilder.SentenceCollector> CreateSentenceCollectors(TranscriptIndex transcript, IReadOnlyDictionary<int, HydratedSentence> hydratedSentences, BookIndex bookIndex, IReadOnlyDictionary<int, int> sentenceToParagraph)
```

**Called-by <-**
- [[PauseMapBuilder.Build]]

