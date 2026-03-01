---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/validation
  - llm/error-handling
---
# SentencePauseMap::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`

## Summary
**Constructs a sentence-level pause map with IDs, original/current timing state, timeline entries, and pause statistics.**

The constructor initializes a `SentencePauseMap` and forwards `stats` to `PauseScopeBase` for base-level validation/storage. It assigns identity and timing fields (`SentenceId`, `ParagraphId`, `OriginalTiming`), initializes `CurrentTiming` to the original timing snapshot, and stores the timeline in a private readonly field. The only direct guard in this constructor is a null check on `timeline` (`ArgumentNullException`), while `stats` validation is handled by the base constructor.


#### [[SentencePauseMap..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public SentencePauseMap(int sentenceId, int paragraphId, SentenceTiming originalTiming, IReadOnlyList<SentenceTimelineElement> timeline, PauseStatsSet stats)
```

