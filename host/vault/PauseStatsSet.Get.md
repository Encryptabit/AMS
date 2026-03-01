---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs"
access_modifier: "public"
complexity: 7
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# PauseStatsSet::Get
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`

## Summary
**Retrieves the pause statistics bucket corresponding to a given `PauseClass`.**

`Get` performs a `switch` expression on `pauseClass` and returns the mapped `PauseStats` property (`Comma`, `Sentence`, `Paragraph`, `ChapterHead`, `PostChapterRead`, `Tail`). Any unrecognized enum value falls through to `_ => Other`, providing a safe default bucket. The method is pure and allocation-free, acting as a typed accessor over the set.


#### [[PauseStatsSet.Get]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public PauseStats Get(PauseClass pauseClass)
```

