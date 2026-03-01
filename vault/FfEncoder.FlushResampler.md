---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 4
tags:
  - method
---
# FfEncoder::FlushResampler
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`


#### [[FfEncoder.FlushResampler]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void FlushResampler(int sourceSampleRate, int targetSampleRate, SwrContext* resampler, AVFrame* frame, AVCodecContext* cc, AVStream* stream, AVFormatContext* fmt, ref long pts)
```

**Calls ->**
- [[FfEncoder.DrainEncoder]]
- [[FfEncoder.EnsureFrameCapacity]]
- [[FfUtils.ComputeResampleOutputSamples]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfEncoder.EncodeBuffer]]
- [[StreamingEncoderSink.Complete]]

