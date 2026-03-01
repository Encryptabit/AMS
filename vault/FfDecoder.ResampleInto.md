---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 3
tags:
  - method
---
# FfDecoder::ResampleInto
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`


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

