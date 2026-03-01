---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioIntegrityVerifier::MapToRaw
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

## Summary
**Translate a treated timestamp into estimated raw time using a piecewise-linear sentence alignment map.**

`MapToRaw` projects a treated-time value `t` onto raw timeline coordinates using precomputed affine map segments (`MapSeg`). It returns `t` directly when `map` is empty; otherwise it performs a binary search on segment start times (`T0`) to select the last segment whose start is `<= t`, then evaluates `A * t + B`. The lookup is O(log n) and avoids linear scans per frame in the verifier loop.


#### [[AudioIntegrityVerifier.MapToRaw]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double MapToRaw(IReadOnlyList<AudioIntegrityVerifier.MapSeg> map, double t)
```

**Called-by <-**
- [[AudioIntegrityVerifier.Verify]]

