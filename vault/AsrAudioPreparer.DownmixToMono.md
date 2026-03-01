---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AsrAudioPreparer.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 5
tags:
  - method
---
# AsrAudioPreparer::DownmixToMono
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AsrAudioPreparer.cs`


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

