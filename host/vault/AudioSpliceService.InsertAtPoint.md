---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioSpliceService.cs"
access_modifier: "public"
complexity: 4
fan_in: 1
fan_out: 5
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioSpliceService::InsertAtPoint
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioSpliceService.cs`

## Summary
**Insert audio at a single timestamp in a buffer and blend both splice boundaries with safe crossfades.**

`InsertAtPoint` inserts `insertion` into `original` at `insertPointSec` by trimming original into `before`/`after` halves at the same split point and crossfading twice. It validates null arguments and `insertPointSec >= 0`, defaults blank `curve` to `"tri"`, and resamples `insertion` to `original.SampleRate` when formats differ. It clamps crossfade duration independently for `before + insertion` and `joined + after` using `ClampCrossfade(crossfadeSec, DurationSeconds(...), DurationSeconds(...))`, then performs the joins with `Crossfade`. The method returns a new spliced buffer and avoids `ReplaceSegment`’s `start < end` constraint.


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

