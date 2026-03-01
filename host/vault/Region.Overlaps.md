---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# Region::Overlaps
**Path**: `Projects/AMS/host/Ams.Core/Audio/FeatureExtraction.cs`

## Summary
**Determine whether two time regions share a non-zero temporal intersection.**

`Overlaps` performs an interval-intersection check using half-open boundary semantics. It returns `false` when this region ends at/before `other.StartSec` or when `other` ends at/before this `StartSec`; otherwise it returns `true`. Because the comparisons are `<=`, touching endpoints are treated as non-overlapping.


#### [[Region.Overlaps]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public bool Overlaps(Region other)
```

