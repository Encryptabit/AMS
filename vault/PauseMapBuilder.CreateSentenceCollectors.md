---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
---
# PauseMapBuilder::CreateSentenceCollectors
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`


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

