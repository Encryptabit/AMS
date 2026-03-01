---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AsrProcessor::ExtractMonoSamples
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Produces a mono PCM sample array from a multi- or single-channel audio buffer.**

`ExtractMonoSamples` converts an `AudioBuffer` into a single-channel `float[]` suitable for language detection. It returns `Array.Empty<float>()` when the buffer has no channels or length, copies channel 0 directly when already mono, and otherwise allocates a mono array and averages all channel samples per frame (`sum / buffer.Channels`). The method performs deterministic downmixing without resampling.


#### [[AsrProcessor.ExtractMonoSamples]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static float[] ExtractMonoSamples(AudioBuffer buffer)
```

**Called-by <-**
- [[AsrProcessor.DetectLanguageInternalAsync]]

