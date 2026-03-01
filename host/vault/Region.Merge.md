---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# Region::Merge
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

## Summary
**Produce a single region that covers the full temporal extent of two regions.**

`Merge` is a pure combiner that returns a new `Region` spanning both inputs by taking `Math.Min(a.StartSec, b.StartSec)` and `Math.Max(a.EndSec, b.EndSec)`. It does not require overlap and performs no mutation or side effects. The result is the minimal bounding interval containing `a` and `b`.


#### [[Region.Merge]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Region Merge(Region a, Region b)
```

**Called-by <-**
- [[FeatureExtraction.MergeAndFilter]]

