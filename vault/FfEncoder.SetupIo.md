---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 1
tags:
  - method
---
# FfEncoder::SetupIo
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`


#### [[FfEncoder.SetupIo]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void SetupIo(AVFormatContext* fmt, Stream output, FfEncoder.EncoderSink sink, ref AVIOContext* customIo, ref GCHandle handle, ref avio_alloc_context_write_packet writeCallback)
```

**Calls ->**
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfEncoder.Encode]]
- [[StreamingEncoderSink.Initialize]]

