---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AsrAudioPreparer.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AsrAudioPreparer::DownmixToMonoSimple
**Path**: `Projects/AMS/host/Ams.Core/Audio/AsrAudioPreparer.cs`

## Summary
**Create a mono audio buffer by averaging each frame across all channels when high-quality FFmpeg mixing is unavailable.**

`DownmixToMonoSimple` implements the non-FFmpeg fallback for channel collapse using direct sample averaging. It short-circuits when input is already mono; otherwise it allocates a new mono `AudioBuffer` with the source sample rate/length and, for each sample index, sums values across all source channels and writes `sum / buffer.Channels` into `mono.Planar[0][i]`. The method preserves temporal length and ordering while discarding channel separation.


#### [[AsrAudioPreparer.DownmixToMonoSimple]]
##### What it does:
<member name="M:Ams.Core.Audio.AsrAudioPreparer.DownmixToMonoSimple(Ams.Core.Artifacts.AudioBuffer)">
    <summary>
    Simple per-sample averaging downmix (fallback when FFmpeg unavailable).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AudioBuffer DownmixToMonoSimple(AudioBuffer buffer)
```

**Called-by <-**
- [[AsrAudioPreparer.DownmixToMono]]

