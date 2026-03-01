---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
---
# AudioTreatmentService::PrepareRoomtoneSegment
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioTreatmentService.cs`


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

