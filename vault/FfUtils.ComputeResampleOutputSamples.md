---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs"
access_modifier: "public"
complexity: 1
fan_in: 4
fan_out: 0
tags:
  - method
---
# FfUtils::ComputeResampleOutputSamples
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`


#### [[FfUtils.ComputeResampleOutputSamples]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static int ComputeResampleOutputSamples(SwrContext* resampler, int sourceSampleRate, int targetSampleRate, int sourceSamples)
```

**Called-by <-**
- [[FfDecoder.ResampleInto]]
- [[FfEncoder.EncodeBuffer]]
- [[FfEncoder.FlushResampler]]
- [[StreamingEncoderSink.Consume]]

