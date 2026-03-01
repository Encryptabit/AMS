---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "internal"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
---
# FfEncoder::CreateStreamingSink
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`


#### [[FfEncoder.CreateStreamingSink]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static FfFilterGraphRunner.IAudioFrameSink CreateStreamingSink(Stream output, AudioEncodeOptions options = null)
```

**Called-by <-**
- [[FfFilterGraph.StreamToWave]]

