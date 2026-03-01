---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfDecoder::AppendSamples
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`

## Summary
**Adds one decoded frame’s audio samples to output channel buffers, optionally routing through resampling.**

This method appends decoded frame samples into the accumulating per-channel float buffers, choosing between passthrough and resample paths. When `needsResample` is true, it delegates to `ResampleInto(...)` and returns immediately. Otherwise it reads planar float data directly from `frame->extended_data[ch]` for `frame->nb_samples` and pushes samples into each `channelSamples[ch]` list.


#### [[FfDecoder.AppendSamples]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void AppendSamples(AVFrame* frame, SwrContext* resampler, bool needsResample, int sourceSampleRate, int targetChannels, int targetSampleRate, AVSampleFormat targetFormat, IList<List<float>> channelSamples, FfDecoder.ResampleScratch scratch)
```

**Calls ->**
- [[FfDecoder.ResampleInto]]

**Called-by <-**
- [[FfDecoder.Decode]]

