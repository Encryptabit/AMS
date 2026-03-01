---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioSpliceService.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
---
# AudioSpliceService::GenerateRoomtoneFill
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioSpliceService.cs`


#### [[AudioSpliceService.GenerateRoomtoneFill]]
##### What it does:
<member name="M:Ams.Core.Audio.AudioSpliceService.GenerateRoomtoneFill(Ams.Core.Artifacts.AudioBuffer,System.Double)">
    <summary>
    Generates a roomtone fill buffer of the specified target duration by looping
    the provided roomtone sample. If the roomtone is already long enough, it is
    trimmed to the exact target length.
    </summary>
    <param name="roomtone">The source roomtone audio buffer to loop.</param>
    <param name="targetDurationSec">The desired fill duration in seconds.</param>
    <returns>A new <see cref="T:Ams.Core.Artifacts.AudioBuffer"/> of exactly <paramref name="targetDurationSec"/> length.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBuffer GenerateRoomtoneFill(AudioBuffer roomtone, double targetDurationSec)
```

**Calls ->**
- [[AudioProcessor.Trim]]

**Called-by <-**
- [[PolishService.ApplyRoomtoneOperationAsync]]

