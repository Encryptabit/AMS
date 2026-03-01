---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs"
access_modifier: "public"
complexity: 2
fan_in: 22
fan_out: 1
tags:
  - method
  - danger/high-fan-in
---
# FfUtils::ThrowIfError
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`

> [!danger] High Fan-In (22)
> This method is called by 22 other methods. Changes here have wide impact.


#### [[FfUtils.ThrowIfError]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void ThrowIfError(int errorCode, string where)
```

**Calls ->**
- [[FfUtils.FormatError]]

**Called-by <-**
- [[FfDecoder.Decode]]
- [[FfDecoder.Probe]]
- [[FfDecoder.ResampleInto]]
- [[ResampleScratch.Rent]]
- [[FfEncoder.AllocateFrame]]
- [[FfEncoder.DrainEncoder]]
- [[FfEncoder.Encode]]
- [[FfEncoder.EncodeBuffer]]
- [[FfEncoder.EnsureFrameCapacity]]
- [[FfEncoder.FinalizeIo]]
- [[FfEncoder.FlushResampler]]
- [[FfEncoder.SetupIo]]
- [[StreamingEncoderSink.Complete]]
- [[StreamingEncoderSink.Consume]]
- [[StreamingEncoderSink.Initialize]]
- [[FilterGraphExecutor.Drain]]
- [[FilterGraphExecutor.Process]]
- [[FilterGraphExecutor.SendFrame]]
- [[FilterGraphExecutor.SetupSink]]
- [[FilterGraphExecutor.SetupSource]]
- [[FfSession.EnsureInitialized]]
- [[FfUtils.CloneOrDefault]]

