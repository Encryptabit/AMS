---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfSession.cs"
access_modifier: "public"
complexity: 4
fan_in: 6
fan_out: 4
tags:
  - method
---
# FfSession::EnsureInitialized
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfSession.cs`


#### [[FfSession.EnsureInitialized]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfSession.EnsureInitialized">
    <summary>
    Ensures FFmpeg has been initialized for the current process.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void EnsureInitialized()
```

**Calls ->**
- [[FfSession.BuildFailureHint]]
- [[FfSession.IsBindingException]]
- [[FfSession.TrySetRootPath]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfDecoder.Decode]]
- [[FfDecoder.Probe]]
- [[FfEncoder.Encode]]
- [[StreamingEncoderSink.Initialize]]
- [[FfSession.EnsureFiltersAvailable]]
- [[WavIoTests.FfmpegUnavailable]]

