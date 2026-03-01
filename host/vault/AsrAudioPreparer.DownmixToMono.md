---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AsrAudioPreparer.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 5
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrAudioPreparer::DownmixToMono
**Path**: `Projects/AMS/host/Ams.Core/Audio/AsrAudioPreparer.cs`

## Summary
**Produce a mono audio buffer from multichannel input, preferring FFmpeg pan-filter mixing and falling back to simple averaging.**

`DownmixToMono` converts multichannel audio to mono with a quality-first fallback strategy. It immediately returns the input when `buffer.Channels == 1`; otherwise, if `FfSession.FiltersAvailable` is true, it runs an FFmpeg filter-graph path (`FfFilterGraph.FromBuffer(buffer).Custom(BuildMonoPanClause(buffer.Channels)).ToBuffer()`) for equal-weight pan mixing. When filters are unavailable, it falls back to `DownmixToMonoSimple(buffer)`, which performs manual per-sample averaging.


#### [[AsrAudioPreparer.DownmixToMono]]
##### What it does:
<member name="M:Ams.Core.Audio.AsrAudioPreparer.DownmixToMono(Ams.Core.Artifacts.AudioBuffer)">
    <summary>
    Downmixes a multi-channel buffer to mono.
    Uses FFmpeg pan filter for high-quality mixing when available,
    falls back to simple per-sample averaging otherwise.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AudioBuffer DownmixToMono(AudioBuffer buffer)
```

**Calls ->**
- [[AsrAudioPreparer.BuildMonoPanClause]]
- [[AsrAudioPreparer.DownmixToMonoSimple]]
- [[FfFilterGraph.Custom]]
- [[FfFilterGraph.FromBuffer]]
- [[FfFilterGraph.ToBuffer]]

**Called-by <-**
- [[AsrAudioPreparer.PrepareForAsr]]

