---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
---
# FfEncoder::EncodeToCustomStream
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`


#### [[FfEncoder.EncodeToCustomStream]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void EncodeToCustomStream(AudioBuffer buffer, Stream output, AudioEncodeOptions options = null)
```

**Calls ->**
- [[FfEncoder.Encode]]

**Called-by <-**
- [[AudioProcessor.EncodeWav]]
- [[AudioProcessor.EncodeWavToStream]]

