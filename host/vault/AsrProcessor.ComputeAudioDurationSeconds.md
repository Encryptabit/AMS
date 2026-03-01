---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AsrProcessor::ComputeAudioDurationSeconds
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Computes audio duration in seconds from an `AudioBuffer`’s sample count and sample rate.**

`ComputeAudioDurationSeconds` derives clip length from raw buffer metadata by dividing `buffer.Length` (sample frames) by `buffer.SampleRate`. It guards invalid sample rates by returning `0` when `SampleRate <= 0`, preventing divide-by-zero and signaling unknown/invalid duration.


#### [[AsrProcessor.ComputeAudioDurationSeconds]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double ComputeAudioDurationSeconds(AudioBuffer buffer)
```

**Called-by <-**
- [[AsrProcessor.ShouldRetryWithoutDtw]]

