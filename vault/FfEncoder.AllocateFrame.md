---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
---
# FfEncoder::AllocateFrame
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`


#### [[FfEncoder.AllocateFrame]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AVFrame* AllocateFrame(AVCodecContext* cc)
```

**Calls ->**
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfEncoder.Encode]]
- [[StreamingEncoderSink.Initialize]]

