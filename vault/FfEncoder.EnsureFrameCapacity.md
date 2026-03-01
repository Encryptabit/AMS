---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 5
fan_in: 3
fan_out: 1
tags:
  - method
---
# FfEncoder::EnsureFrameCapacity
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`


#### [[FfEncoder.EnsureFrameCapacity]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void EnsureFrameCapacity(AVFrame* frame, AVCodecContext* cc, int requiredSamples)
```

**Calls ->**
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfEncoder.EncodeBuffer]]
- [[FfEncoder.FlushResampler]]
- [[StreamingEncoderSink.Consume]]

