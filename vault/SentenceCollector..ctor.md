---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 2
tags:
  - method
---
# SentenceCollector::.ctor
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`


#### [[SentenceCollector..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public SentenceCollector(SentenceAlign sentence, HydratedSentence hydrated, int paragraphId, BookIndex bookIndex)
```

**Calls ->**
- [[SentenceCollector.BuildWordTimeline]]
- [[SentenceCollector.ResolveTiming]]

