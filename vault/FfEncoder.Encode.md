---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 13
fan_in: 2
fan_out: 12
tags:
  - method
---
# FfEncoder::Encode
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`


#### [[FfEncoder.Encode]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void Encode(AudioBuffer buffer, Stream output, AudioEncodeOptions options, FfEncoder.EncoderSink sink)
```

**Calls ->**
- [[FfEncoder.AllocateFrame]]
- [[FfEncoder.CleanupIo]]
- [[FfEncoder.DrainEncoder]]
- [[FfEncoder.EncodeBuffer]]
- [[FfEncoder.FinalizeIo]]
- [[FfEncoder.PinChannels]]
- [[FfEncoder.ResolveEncoding]]
- [[FfEncoder.SetupIo]]
- [[FfEncoder.UnpinChannels]]
- [[FfSession.EnsureInitialized]]
- [[FfUtils.CreateDefaultChannelLayout]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfEncoder.EncodeToCustomStream]]
- [[FfEncoder.EncodeToDynamicBuffer]]

