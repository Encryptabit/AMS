---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
---
# FfEncoder::ResolveEncoding
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`


#### [[FfEncoder.ResolveEncoding]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (AVCodecID CodecId, AVSampleFormat SampleFormat) ResolveEncoding(int bitDepth)
```

**Called-by <-**
- [[FfEncoder.Encode]]
- [[StreamingEncoderSink.Initialize]]

