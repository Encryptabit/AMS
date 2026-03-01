---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioSpliceService.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioSpliceService::GenerateRoomtoneFill
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioSpliceService.cs`

## Summary
**Produce an exact-duration roomtone buffer by trimming or looping the provided roomtone audio.**

`GenerateRoomtoneFill` creates a buffer with exact target duration from a source roomtone clip. It null-checks input, returns a zero-length buffer (preserving channel/sample-rate/metadata) when `targetDurationSec <= 0`, computes `targetSamples`, and if the source is already long enough delegates to `AudioProcessor.Trim(..., 0, targetDurationSec)`. Otherwise it allocates a new destination buffer and loops each channel, repeatedly `Array.Copy`-ing the full roomtone channel (or the remaining tail) until `targetSamples` is filled. The output preserves source format while ensuring deterministic exact-length fill.


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

