---
namespace: "Ams.Tests"
project: "Ams.Tests"
source_file: "Projects/AMS/host/Ams.Tests/AudioProcessorFilterTests.cs"
access_modifier: "private"
complexity: 4
fan_in: 3
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/factory
---
# AudioProcessorFilterTests::CreateBuffer
**Path**: `Projects/AMS/host/Ams.Tests/AudioProcessorFilterTests.cs`

## Summary
**Generate a contiguous in-memory audio buffer from frequency-duration segments for filter behavior tests.**

`CreateBuffer` builds a synthetic mono test signal at a fixed 16 kHz sample rate by precomputing `totalSamples` as `Round(sum(seconds) * sampleRate)` and allocating `new AudioBuffer(1, 16000, totalSamples)`. It iterates each `(frequency, seconds)` segment, converts duration to sample count, writes either zeros (`frequency == 0`) or a 0.5-scale sine wave (`Math.Sin(2 * PI * f * i / sampleRate)`) into `buffer.Planar[0]` at `cursor + i`, then advances the cursor. The method is a deterministic test helper for constructing mixed silence/tone buffers used by trim/fade/silence-detection tests.


#### [[AudioProcessorFilterTests.CreateBuffer]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AudioBuffer CreateBuffer(params (double frequency, double seconds)[] segments)
```

**Called-by <-**
- [[AudioProcessorFilterTests.DetectSilence_FindsInitialGap]]
- [[AudioProcessorFilterTests.FadeIn_GraduallyIncreasesAmplitude]]
- [[AudioProcessorFilterTests.Trim_ReturnsExpectedSegment]]

