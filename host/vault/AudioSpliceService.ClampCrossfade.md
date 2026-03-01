---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioSpliceService.cs"
access_modifier: "private"
complexity: 1
fan_in: 3
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AudioSpliceService::ClampCrossfade
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioSpliceService.cs`

## Summary
**Limit crossfade length to a boundary-safe value based on both segment durations.**

`ClampCrossfade` computes a safe fade duration by taking the minimum of the requested value and 30% of each side’s duration: `Math.Min(requestedSec, Math.Min(aDurationSec * 0.3, bDurationSec * 0.3))`. This enforces boundary-safe overlap so fades cannot consume too much of either segment. The method is deterministic, allocation-free, and used uniformly by splice operations.


#### [[AudioSpliceService.ClampCrossfade]]
##### What it does:
<member name="M:Ams.Core.Audio.AudioSpliceService.ClampCrossfade(System.Double,System.Double,System.Double)">
    <summary>
    Clamps the crossfade duration to 30% of the shorter of the two segments,
    preventing the crossfade from exceeding segment boundaries.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double ClampCrossfade(double requestedSec, double aDurationSec, double bDurationSec)
```

**Called-by <-**
- [[AudioSpliceService.DeleteRegion]]
- [[AudioSpliceService.InsertAtPoint]]
- [[AudioSpliceService.ReplaceSegment]]

