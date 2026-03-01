---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioSpliceService.cs"
access_modifier: "private"
complexity: 4
fan_in: 3
fan_out: 2
tags:
  - method
---
# AudioSpliceService::Crossfade
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioSpliceService.cs`


#### [[AudioSpliceService.Crossfade]]
##### What it does:
<member name="M:Ams.Core.Audio.AudioSpliceService.Crossfade(Ams.Core.Artifacts.AudioBuffer,Ams.Core.Artifacts.AudioBuffer,System.Double,System.String)">
    <summary>
    Crossfades two audio buffers using FFmpeg's acrossfade filter.
    Falls back to simple concatenation when crossfade is negligible or a buffer is empty.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AudioBuffer Crossfade(AudioBuffer a, AudioBuffer b, double durationSec, string curve)
```

**Calls ->**
- [[AudioBuffer.Concat]]
- [[FfFilterGraphRunner.Apply_2]]

**Called-by <-**
- [[AudioSpliceService.DeleteRegion]]
- [[AudioSpliceService.InsertAtPoint]]
- [[AudioSpliceService.ReplaceSegment]]

