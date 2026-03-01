---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseCompressionMath::TryGetBounds
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseCompressionMath.cs`

## Summary
**Resolves pause-duration bounds for a pause class from the current pause policy.**

`TryGetBounds` maps a `PauseClass` to policy-derived `PauseBounds` via a `switch`, returning `true` when a mapping exists and setting the `out` value accordingly. Comma/Sentence/Paragraph use their policy window min/max pairs, while ChapterHead/PostChapterRead/Tail map to fixed bounds where min and max are the same scalar policy value. Unsupported classes set `bounds = default` and return `false`.


#### [[PauseCompressionMath.TryGetBounds]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryGetBounds(PauseClass class, PausePolicy policy, out PauseCompressionMath.PauseBounds bounds)
```

**Called-by <-**
- [[PauseCompressionMath.BuildProfiles]]

