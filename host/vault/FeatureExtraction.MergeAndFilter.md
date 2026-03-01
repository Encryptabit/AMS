---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# FeatureExtraction::MergeAndFilter
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

## Summary
**Sort, merge temporally close regions, and remove segments shorter than a minimum duration threshold.**

`MergeAndFilter(List<Region> regions, double mergeGapSec, double minDurationSec)` first sorts regions by `StartSec`, then performs a single forward pass to coalesce adjacent segments whose inter-gap is `<= mergeGapSec`. It builds a new `merged` list, merging with the current tail via `Region.Merge(last, current)` when overlap/near-contiguity criteria are met, otherwise appending a new segment. After merging, it prunes short results in place with `RemoveAll(region => region.DurationSec < minDurationSec)` and returns the filtered list.


#### [[FeatureExtraction.MergeAndFilter]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<Region> MergeAndFilter(List<Region> regions, double mergeGapSec, double minDurationSec)
```

**Calls ->**
- [[Region.Merge]]

**Called-by <-**
- [[FeatureExtraction.Detect_2]]

