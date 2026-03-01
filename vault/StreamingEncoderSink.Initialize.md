---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "public"
complexity: 9
fan_in: 0
fan_out: 6
tags:
  - method
---
# StreamingEncoderSink::Initialize
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`


#### [[StreamingEncoderSink.Initialize]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Initialize(AudioBufferMetadata templateMetadata, int sampleRate, int channels)
```

**Calls ->**
- [[FfEncoder.AllocateFrame]]
- [[FfEncoder.ResolveEncoding]]
- [[FfEncoder.SetupIo]]
- [[FfSession.EnsureInitialized]]
- [[FfUtils.CreateDefaultChannelLayout]]
- [[FfUtils.ThrowIfError]]

