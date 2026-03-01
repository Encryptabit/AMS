---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
---
# FfDecoder::AppendSamples
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`


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

