---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioSpliceService.cs"
access_modifier: "private"
complexity: 1
fan_in: 3
fan_out: 0
tags:
  - method
---
# AudioSpliceService::DurationSeconds
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioSpliceService.cs`


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

