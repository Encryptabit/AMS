---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs"
access_modifier: "public"
complexity: 4
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ChapterPauseMap::UpdateBounds
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseMapModels.cs`

## Summary
**Validates and sets the chapter’s current start/end bounds while enforcing a non-negative interval.**

`UpdateBounds` updates `ChapterPauseMap`’s mutable bounds (`CurrentStart`, `CurrentEnd`) after validating the inputs. It throws `ArgumentOutOfRangeException` if `start` or `end` is non-finite, and normalizes reversed intervals by clamping `end` up to `start` when `end < start`. The method performs no timeline/statistics recalculation and only mutates current boundary state.


#### [[ChapterPauseMap.UpdateBounds]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void UpdateBounds(double start, double end)
```

