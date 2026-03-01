---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AudioAccumulator::Add
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Consumes an output frame and accumulates its interleaved float samples into channel-separated buffers while establishing sample rate if needed.**

`Add` appends one decoded FFmpeg frame into the accumulator’s per-channel float lists. It reads `channels` from `frame->ch_layout.nb_channels` and `samples` from `frame->nb_samples`, lazily initializes `_sampleRate` from `frame->sample_rate` when it was previously unset, then treats `frame->data[0]` as interleaved `float*` PCM. The method iterates samples and channels, computes `baseIndex = i * channels`, and pushes each sample into `_channels[ch]`.


#### [[AudioAccumulator.Add]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Add(AVFrame* frame)
```

**Called-by <-**
- [[FilterGraphExecutor.Drain]]

