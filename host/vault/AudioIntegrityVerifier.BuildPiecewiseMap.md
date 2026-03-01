---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AudioIntegrityVerifier::BuildPiecewiseMap
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`

## Summary
**Build a piecewise-linear time transform from treated timeline coordinates to raw timeline coordinates using aligned sentence timings.**

`BuildPiecewiseMap` constructs treated-to-raw linear mapping segments from sentence timing pairs that share the same sentence ID. It intersects `rawById` and `treatedById` keys, iterates IDs in order, and for each pair computes a segment over treated span `[t0, t1]` with affine coefficients `a` and `b` such that `rawTime = a * treatedTime + b`; zero-length treated spans get `a = 0`. It normalizes ends with `Math.Max(start, end)` and returns the resulting `MapSeg` list sorted by treated start time (`T0`).


#### [[AudioIntegrityVerifier.BuildPiecewiseMap]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<AudioIntegrityVerifier.MapSeg> BuildPiecewiseMap(IReadOnlyDictionary<int, SentenceTiming> rawById, IReadOnlyDictionary<int, SentenceTiming> treatedById)
```

**Called-by <-**
- [[AudioIntegrityVerifier.Verify]]

