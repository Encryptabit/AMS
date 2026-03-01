---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AsrAudioPreparer.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
---
# AsrAudioPreparer::DownmixToMonoSimple
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AsrAudioPreparer.cs`


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

