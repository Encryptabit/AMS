---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioSpliceService.cs"
access_modifier: "public"
complexity: 5
fan_in: 4
fan_out: 5
tags:
  - method
---
# AudioSpliceService::ReplaceSegment
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioSpliceService.cs`


#### [[AudioSpliceService.ReplaceSegment]]
##### What it does:
<member name="M:Ams.Core.Audio.AudioSpliceService.ReplaceSegment(Ams.Core.Artifacts.AudioBuffer,System.Double,System.Double,Ams.Core.Artifacts.AudioBuffer,System.Double,System.String)">
    <summary>
    Replaces the audio between <paramref name="startSec"/> and <paramref name="endSec"/>
    in <paramref name="original"/> with <paramref name="replacement"/>, applying crossfade
    transitions at both splice points.
    </summary>
    <param name="original">The full original audio buffer.</param>
    <param name="startSec">Start of the segment to replace (seconds).</param>
    <param name="endSec">End of the segment to replace (seconds).</param>
    <param name="replacement">The replacement audio buffer to splice in.</param>
    <param name="crossfadeSec">Crossfade duration in seconds (default 30ms). Clamped to avoid exceeding segment boundaries.</param>
    <param name="curve">Crossfade curve type (default "tri" for triangular).</param>
    <returns>A new <see cref="T:Ams.Core.Artifacts.AudioBuffer"/> with the replacement spliced in.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBuffer ReplaceSegment(AudioBuffer original, double startSec, double endSec, AudioBuffer replacement, double crossfadeSec = 0.03, string curve = "tri")
```

**Calls ->**
- [[AudioSpliceService.ClampCrossfade]]
- [[AudioSpliceService.Crossfade]]
- [[AudioSpliceService.DurationSeconds]]
- [[AudioProcessor.Resample]]
- [[AudioProcessor.Trim]]

**Called-by <-**
- [[PolishService.ApplyReplacementAsync]]
- [[PolishService.ApplyRoomtoneOperationAsync]]
- [[PolishService.GeneratePreview]]
- [[PolishService.RevertReplacementAsync]]

