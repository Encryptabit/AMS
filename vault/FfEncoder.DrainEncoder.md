---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 5
fan_in: 5
fan_out: 1
tags:
  - method
---
# FfEncoder::DrainEncoder
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`


#### [[FfEncoder.DrainEncoder]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void DrainEncoder(AVCodecContext* cc, AVStream* stream, AVFormatContext* fmt)
```

**Calls ->**
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfEncoder.Encode]]
- [[FfEncoder.EncodeBuffer]]
- [[FfEncoder.FlushResampler]]
- [[StreamingEncoderSink.Complete]]
- [[StreamingEncoderSink.Consume]]

