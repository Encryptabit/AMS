---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Artifacts/AudioBuffer.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# AudioBuffer::Concat
**Path**: `home/cari/repos/AMS/host/Ams.Core/Artifacts/AudioBuffer.cs`


#### [[AudioBuffer.Concat]]
##### What it does:
<member name="M:Ams.Core.Artifacts.AudioBuffer.Concat(Ams.Core.Artifacts.AudioBuffer[])">
    <summary>
    Concatenates multiple AudioBuffer instances into a single new buffer.
    All buffers must have matching SampleRate and Channels.
    </summary>
    <param name="buffers">The buffers to concatenate in order.</param>
    <returns>A new AudioBuffer containing all samples sequentially.</returns>
    <exception cref="T:System.ArgumentException">Thrown if buffers is empty.</exception>
    <exception cref="T:System.InvalidOperationException">Thrown if buffers have mismatched SampleRate or Channels.</exception>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBuffer Concat(params AudioBuffer[] buffers)
```

**Calls ->**
- [[AudioBuffer.Concat_2]]

**Called-by <-**
- [[AudioSpliceService.Crossfade]]

