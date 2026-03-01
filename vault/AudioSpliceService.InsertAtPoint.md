---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioSpliceService.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 5
tags:
  - method
---
# AudioSpliceService::InsertAtPoint
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioSpliceService.cs`


#### [[AudioSpliceService.InsertAtPoint]]
##### What it does:
<member name="M:Ams.Core.Audio.AudioSpliceService.InsertAtPoint(Ams.Core.Artifacts.AudioBuffer,System.Double,Ams.Core.Artifacts.AudioBuffer,System.Double,System.String)">
    <summary>
    Inserts audio at a single time point in the original buffer, applying crossfade
    transitions at both splice points. This avoids the start==end validation issue
    in <see cref="M:Ams.Core.Audio.AudioSpliceService.ReplaceSegment(Ams.Core.Artifacts.AudioBuffer,System.Double,System.Double,Ams.Core.Artifacts.AudioBuffer,System.Double,System.String)"/> by splitting the original at a single point.
    </summary>
    <param name="original">The full original audio buffer.</param>
    <param name="insertPointSec">The time point (seconds) at which to insert audio.</param>
    <param name="insertion">The audio buffer to insert.</param>
    <param name="crossfadeSec">Crossfade duration in seconds (default 30ms).</param>
    <param name="curve">Crossfade curve type (default "tri" for triangular).</param>
    <returns>A new <see cref="T:Ams.Core.Artifacts.AudioBuffer"/> with the insertion spliced in.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBuffer InsertAtPoint(AudioBuffer original, double insertPointSec, AudioBuffer insertion, double crossfadeSec = 0.03, string curve = "tri")
```

**Calls ->**
- [[AudioSpliceService.ClampCrossfade]]
- [[AudioSpliceService.Crossfade]]
- [[AudioSpliceService.DurationSeconds]]
- [[AudioProcessor.Resample]]
- [[AudioProcessor.Trim]]

**Called-by <-**
- [[PolishService.ApplyRoomtoneOperationAsync]]

