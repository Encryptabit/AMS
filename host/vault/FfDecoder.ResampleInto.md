---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfDecoder::ResampleInto
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`

## Summary
**Resamples a decoded FFmpeg frame into target audio format and appends the result to channel sample buffers.**

This helper performs one frame’s sample-rate/channel-format conversion through FFmpeg `swr_convert` and appends the converted planar floats into per-channel accumulators. It first computes required destination sample capacity via `ComputeResampleOutputSamples`, rents conversion buffers from `ResampleScratch.Rent`, then executes `swr_convert` and validates its return with `ThrowIfError`. The method iterates each output channel and sample index, copying converted `float*` data into `channelSamples[ch]`.


#### [[FfDecoder.ResampleInto]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void ResampleInto(SwrContext* resampler, int sourceSampleRate, AVFrame* frame, int targetChannels, int targetSampleRate, AVSampleFormat targetFormat, IList<List<float>> channelSamples, FfDecoder.ResampleScratch scratch)
```

**Calls ->**
- [[ResampleScratch.Rent]]
- [[FfUtils.ComputeResampleOutputSamples]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfDecoder.AppendSamples]]

