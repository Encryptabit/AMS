---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs"
access_modifier: "public"
complexity: 6
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/di
  - llm/validation
  - llm/error-handling
---
# ChapterPauseMap::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`

## Summary
**Creates a chapter-level pause map with validated time bounds, timeline/paragraph collections, and initial current bounds.**

The constructor initializes a `ChapterPauseMap` and passes `stats` to `PauseScopeBase` for base-level stats wiring. It validates `originalStart`/`originalEnd` with `double.IsFinite`, throws `ArgumentOutOfRangeException` on invalid numbers, and clamps inverted ranges by setting `originalEnd = originalStart` when needed. It null-checks and stores `timeline` and `paragraphs`, then sets both original and current chapter bounds to the validated values.


#### [[ChapterPauseMap..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ChapterPauseMap(IReadOnlyList<ChapterTimelineElement> timeline, IReadOnlyList<ParagraphPauseMap> paragraphs, PauseStatsSet stats, double originalStart, double originalEnd)
```

