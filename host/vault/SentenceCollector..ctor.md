---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/utility
---
# SentenceCollector::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`

## Summary
**Creates a sentence-level pause collector with resolved timing metadata and an initialized word timeline scaffold.**

The `SentenceCollector` constructor initializes collector identity/context from inputs by assigning `SentenceId`, `ParagraphId`, and resolving `OriginalTiming` via `ResolveTiming(sentence, hydrated)`. It then precomputes the base word timeline by invoking `BuildWordTimeline(sentence, bookIndex)`, which seeds internal timeline state used for later pause merges. No pause stats are added at construction time.


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

