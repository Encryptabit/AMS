---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# FeatureExtraction::MergeAndFilter
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`


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

