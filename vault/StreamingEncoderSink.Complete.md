---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 4
tags:
  - method
---
# StreamingEncoderSink::Complete
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`


#### [[StreamingEncoderSink.Complete]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Complete()
```

**Calls ->**
- [[FfEncoder.DrainEncoder]]
- [[FfEncoder.FinalizeIo]]
- [[FfEncoder.FlushResampler]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[StreamingEncoderSink.Dispose]]

