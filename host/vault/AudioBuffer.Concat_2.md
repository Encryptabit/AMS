---
namespace: "Ams.Core.Artifacts"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Artifacts/AudioBuffer.cs"
access_modifier: "public"
complexity: 12
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioBuffer::Concat
**Path**: `Projects/AMS/host/Ams.Core/Artifacts/AudioBuffer.cs`

## Summary
**Concatenate a sequence of audio buffers into one new buffer with strict format compatibility checks and ordered sample copying.**

`Concat(IEnumerable<AudioBuffer> buffers)` materializes the input as an `IList` (or `ToList`) after a null guard, then enforces non-empty input and throws `ArgumentException` when empty. For a single item, it returns a deep clone by allocating a new `AudioBuffer` with matching format/metadata and copying each channel via `Array.Copy`. For multiple buffers, it validates uniform `SampleRate` and `Channels` (throwing `InvalidOperationException` with index-specific diagnostics on mismatch), sums lengths in `long` with overflow/`int.MaxValue` checks, allocates one destination buffer, and sequentially copies each source channel into the destination using per-channel write offsets. The returned buffer preserves the first buffer’s format and metadata while containing all samples concatenated in order.


#### [[AudioBuffer.Concat_2]]
##### What it does:
<member name="M:Ams.Core.Artifacts.AudioBuffer.Concat(System.Collections.Generic.IEnumerable{Ams.Core.Artifacts.AudioBuffer})">
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
public static AudioBuffer Concat(IEnumerable<AudioBuffer> buffers)
```

**Called-by <-**
- [[AudioBuffer.Concat]]
- [[AudioTreatmentService.TreatChapterCoreAsync]]

