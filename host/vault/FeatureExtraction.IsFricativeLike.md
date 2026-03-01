---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# FeatureExtraction::IsFricativeLike
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

## Summary
**Determine whether a phonetic label should be treated as fricative-like when building protection masks.**

`IsFricativeLike(string label)` classifies phoneme labels against a hardcoded fricative set after input normalization. It first guards with `string.IsNullOrWhiteSpace` and returns `false` for empty/blank labels, then lowercases using `ToLowerInvariant()` for case-insensitive matching. A switch expression returns `true` for specific IPA and digraph tokens (`s/z/ʃ/ʒ/f/v/θ/ð/h/ɕ/ʑ`, plus `sh/zh/ch/jh`) and `false` otherwise.


#### [[FeatureExtraction.IsFricativeLike]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsFricativeLike(string label)
```

**Called-by <-**
- [[FeatureExtraction.BuildProtectionMask]]

