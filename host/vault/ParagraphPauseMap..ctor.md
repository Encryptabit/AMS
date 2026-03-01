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
# ParagraphPauseMap::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`

## Summary
**Constructs a paragraph-level pause map with timeline/sentence references, validated original bounds, and initial current bounds.**

The constructor initializes `ParagraphPauseMap` state and delegates `stats` initialization to `PauseScopeBase` via `: base(stats)`. It validates temporal bounds with `double.IsFinite`, throws `ArgumentOutOfRangeException` for non-finite inputs, and normalizes inverted ranges by clamping `originalEnd` to `originalStart`. It then assigns `ParagraphId`, stores `timeline` and `sentences` with null guards (`ArgumentNullException`), and initializes both original and current bounds to the validated values.


#### [[ParagraphPauseMap..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ParagraphPauseMap(int paragraphId, IReadOnlyList<ParagraphTimelineElement> timeline, IReadOnlyList<SentencePauseMap> sentences, PauseStatsSet stats, double originalStart, double originalEnd)
```

