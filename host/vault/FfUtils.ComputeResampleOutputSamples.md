---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs"
access_modifier: "public"
complexity: 1
fan_in: 4
fan_out: 0
tags:
  - method
  - llm/utility
---
# FfUtils::ComputeResampleOutputSamples
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`

## Summary
**Calculates a conservative output-sample count for a resampler by combining current resampler delay with incoming samples and converting to the target sample rate.**

`ComputeResampleOutputSamples` is an `unsafe` FFmpeg interop helper that estimates how many output samples a resample operation may produce. It calls `swr_get_delay(resampler, sourceSampleRate)` to include buffered converter latency, adds `sourceSamples`, then scales from source to target rate via `av_rescale_rnd(..., AV_ROUND_UP)` and casts to `int`, intentionally rounding up to get a safe destination capacity. The method performs no argument validation and depends on callers to pass a valid `SwrContext*` and rates.


#### [[FfUtils.ComputeResampleOutputSamples]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static int ComputeResampleOutputSamples(SwrContext* resampler, int sourceSampleRate, int targetSampleRate, int sourceSamples)
```

**Called-by <-**
- [[FfDecoder.ResampleInto]]
- [[FfEncoder.EncodeBuffer]]
- [[FfEncoder.FlushResampler]]
- [[StreamingEncoderSink.Consume]]

