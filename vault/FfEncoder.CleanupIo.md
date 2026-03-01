---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 10
fan_in: 2
fan_out: 0
tags:
  - method
---
# FfEncoder::CleanupIo
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`


#### [[FfEncoder.CleanupIo]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void CleanupIo(AVFormatContext* fmt, FfEncoder.EncoderSink sink, ref AVIOContext* customIo, ref GCHandle handle, avio_alloc_context_write_packet writeCallback)
```

**Called-by <-**
- [[FfEncoder.Encode]]
- [[StreamingEncoderSink.Dispose]]

