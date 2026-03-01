---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 4
fan_in: 3
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioProcessor::Resample
**Path**: `Projects/AMS/host/Ams.Core/Processors/AudioProcessor.cs`

## Summary
**Converts an audio buffer to a target sample rate using an FFmpeg-backed filter pipeline with guard checks and no-op fast path.**

`Resample` validates inputs by throwing `ArgumentNullException` when `buffer` is null and `ArgumentOutOfRangeException` when `targetSampleRate <= 0`. It short-circuits by returning the original buffer when the sample rate already matches. Otherwise it builds an FFmpeg filter graph from the buffer (`FfFilterGraph.FromBuffer`), applies a resample filter (`graph.Resample(new ResampleFilterParams(targetSampleRate))`), and materializes the transformed audio via `graph.ToBuffer()`.


#### [[AudioProcessor.Resample]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBuffer Resample(AudioBuffer buffer, ulong targetSampleRate)
```

**Calls ->**
- [[FfFilterGraph.FromBuffer]]
- [[FfFilterGraph.Resample]]
- [[FfFilterGraph.ToBuffer]]

**Called-by <-**
- [[AsrAudioPreparer.PrepareForAsr]]
- [[AudioSpliceService.InsertAtPoint]]
- [[AudioSpliceService.ReplaceSegment]]

