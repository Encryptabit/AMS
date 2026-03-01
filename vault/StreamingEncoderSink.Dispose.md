---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "public"
complexity: 5
fan_in: 0
fan_out: 2
tags:
  - method
---
# StreamingEncoderSink::Dispose
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`


#### [[StreamingEncoderSink.Dispose]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Dispose()
```

**Calls ->**
- [[FfEncoder.CleanupIo]]
- [[StreamingEncoderSink.Complete]]

