---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioSpliceService.cs"
access_modifier: "private"
complexity: 1
fan_in: 3
fan_out: 0
tags:
  - method
---
# AudioSpliceService::ClampCrossfade
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioSpliceService.cs`


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

