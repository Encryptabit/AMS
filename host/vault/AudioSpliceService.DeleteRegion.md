---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioSpliceService.cs"
access_modifier: "public"
complexity: 8
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioSpliceService::DeleteRegion
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioSpliceService.cs`

## Summary
**Remove a specified region from audio and reconnect the remaining parts with a bounded crossfade transition.**

`DeleteRegion` excises a time span from `original` by trimming `before = [0,startSec)` and `after = [endSec,∞)` and then joining the halves. It validates arguments (`startSec >= 0`, `endSec > startSec`), defaults blank `curve` to `"tri"`, and handles boundary cases explicitly: both halves empty yields a new zero-length buffer (preserving format/metadata), otherwise it returns the non-empty half when one side is empty. When both halves exist, it computes a safe fade length with `ClampCrossfade(crossfadeSec, DurationSeconds(before), DurationSeconds(after))` and uses `Crossfade(before, after, clampedCrossfade, curve)` for the final splice. The method is non-mutating and returns a new `AudioBuffer` result.


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

