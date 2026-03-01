---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/di
---
# PauseStatsSet::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`

## Summary
**Initializes a `PauseStatsSet` with precomputed pause statistics for each pause class bucket.**

This constructor is a direct value initializer that assigns each incoming `PauseStats` argument to its corresponding read-only property (`Comma`, `Sentence`, `Paragraph`, `ChapterHead`, `PostChapterRead`, `Tail`, `Other`). It performs no validation, transformation, or allocation beyond normal object construction, so all semantics depend on caller-provided instances.


#### [[PauseStatsSet..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PauseStatsSet(PauseStats comma, PauseStats sentence, PauseStats paragraph, PauseStats chapterHead, PauseStats postChapterRead, PauseStats tail, PauseStats other)
```

