---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioSpliceService.cs"
access_modifier: "public"
complexity: 5
fan_in: 4
fan_out: 5
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioSpliceService::ReplaceSegment
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioSpliceService.cs`

## Summary
**Produce a new audio buffer where a target time range is replaced with another buffer and blended at both splice boundaries.**

`ReplaceSegment` validates inputs/time bounds (`startSec >= 0`, `endSec > startSec`) and normalizes an empty/whitespace curve to `"tri"`. It resamples `replacement` to `original.SampleRate` when needed, slices `original` into `before` (`[0,startSec)`) and `after` (`[endSec,∞)`) via `AudioProcessor.Trim`, then performs two sequential joins with clamped crossfades. Each crossfade duration is independently bounded by `ClampCrossfade(crossfadeSec, DurationSeconds(left), DurationSeconds(right))`, first for `before + replacement`, then for `joined + after`, and both joins use `Crossfade(...)`. The method returns a newly spliced buffer rather than mutating either input.


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

