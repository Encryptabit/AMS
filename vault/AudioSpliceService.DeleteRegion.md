---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioSpliceService.cs"
access_modifier: "public"
complexity: 8
fan_in: 1
fan_out: 4
tags:
  - method
---
# AudioSpliceService::DeleteRegion
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioSpliceService.cs`


#### [[AudioSpliceService.DeleteRegion]]
##### What it does:
<member name="M:Ams.Core.Audio.AudioSpliceService.DeleteRegion(Ams.Core.Artifacts.AudioBuffer,System.Double,System.Double,System.Double,System.String)">
    <summary>
    Removes the audio between <paramref name="startSec"/> and <paramref name="endSec"/>
    and joins the remaining before/after segments with a crossfade transition.
    This avoids the zero-length replacement buffer issue by directly joining the two halves.
    </summary>
    <param name="original">The full original audio buffer.</param>
    <param name="startSec">Start of the region to delete (seconds).</param>
    <param name="endSec">End of the region to delete (seconds).</param>
    <param name="crossfadeSec">Crossfade duration in seconds (default 30ms).</param>
    <param name="curve">Crossfade curve type (default "tri" for triangular).</param>
    <returns>A new <see cref="T:Ams.Core.Artifacts.AudioBuffer"/> with the region removed.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBuffer DeleteRegion(AudioBuffer original, double startSec, double endSec, double crossfadeSec = 0.03, string curve = "tri")
```

**Calls ->**
- [[AudioSpliceService.ClampCrossfade]]
- [[AudioSpliceService.Crossfade]]
- [[AudioSpliceService.DurationSeconds]]
- [[AudioProcessor.Trim]]

**Called-by <-**
- [[PolishService.ApplyRoomtoneOperationAsync]]

