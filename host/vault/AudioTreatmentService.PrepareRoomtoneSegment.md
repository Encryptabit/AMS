---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioTreatmentService::PrepareRoomtoneSegment
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs`

## Summary
**Generate an exact-length roomtone segment by repeating the source roomtone audio across channels.**

`PrepareRoomtoneSegment` creates a fixed-duration roomtone buffer by validating inputs and then looping source samples. It throws `InvalidOperationException` if `roomtone.Length == 0`, throws `ArgumentOutOfRangeException` when `durationSeconds <= 0`, logs source/target timing with `Log.Debug`, computes `targetSamples = (int)(durationSeconds * roomtone.SampleRate)`, and guards against non-positive sample counts. It allocates a new `AudioBuffer` with matching channel/sample-rate and fills each target channel via modulo indexing (`source[i % sourceLen]`) to repeat roomtone seamlessly. The implementation always loop-fills rather than trimming.


#### [[AudioTreatmentService.PrepareRoomtoneSegment]]
##### What it does:
<member name="M:Ams.Core.Audio.AudioTreatmentService.PrepareRoomtoneSegment(Ams.Core.Artifacts.AudioBuffer,System.Double)">
    <summary>
    Prepares a roomtone segment of the specified duration.
    If roomtone is shorter than needed, loops it.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AudioBuffer PrepareRoomtoneSegment(AudioBuffer roomtone, double durationSeconds)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[AudioTreatmentService.TreatChapterCoreAsync]]

