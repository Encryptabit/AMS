---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/AudioBuffer.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# AudioBuffer::Concat
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/AudioBuffer.cs`

## Summary
**Provide a variadic entry point for concatenating multiple `AudioBuffer` instances by delegating to the main `IEnumerable`-based implementation.**

`Concat(params AudioBuffer[] buffers)` is a convenience overload that forwards directly to the enumerable implementation via `Concat((IEnumerable<AudioBuffer>)buffers)`. It adds no concatenation logic of its own; all validation (empty input, format compatibility) and sample-copy behavior are handled by the called overload. Its sole role is enabling `params` call syntax for variadic buffer inputs.


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

