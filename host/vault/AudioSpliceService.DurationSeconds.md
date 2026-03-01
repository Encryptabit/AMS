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
---
# AudioSpliceService::DurationSeconds
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioSpliceService.cs`

## Summary
**Return an `AudioBuffer`’s playback duration in seconds from its sample length and sample rate.**

`DurationSeconds` is a pure helper that computes buffer length in seconds by dividing sample count by sample rate: `(double)buffer.Length / buffer.SampleRate`. It performs no allocation or side effects and assumes the buffer carries valid format metadata. The value is used to bound crossfade durations in splice operations.


#### [[AudioSpliceService.DurationSeconds]]
##### What it does:
<member name="M:Ams.Core.Audio.AudioSpliceService.DurationSeconds(Ams.Core.Artifacts.AudioBuffer)">
    <summary>
    Computes the duration in seconds of an <see cref="T:Ams.Core.Artifacts.AudioBuffer"/>.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double DurationSeconds(AudioBuffer buffer)
```

**Called-by <-**
- [[AudioSpliceService.DeleteRegion]]
- [[AudioSpliceService.InsertAtPoint]]
- [[AudioSpliceService.ReplaceSegment]]

