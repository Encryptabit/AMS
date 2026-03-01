---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "public"
complexity: 6
fan_in: 0
fan_out: 4
tags:
  - method
---
# StreamingEncoderSink::Consume
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`


#### [[StreamingEncoderSink.Consume]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Consume(AVFrame* frame)
```

**Calls ->**
- [[FfEncoder.DrainEncoder]]
- [[FfEncoder.EnsureFrameCapacity]]
- [[FfUtils.ComputeResampleOutputSamples]]
- [[FfUtils.ThrowIfError]]

