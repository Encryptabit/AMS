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
# FfUtils::CreateDefaultChannelLayout
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`


#### [[FfUtils.CreateDefaultChannelLayout]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AVChannelLayout CreateDefaultChannelLayout(int channels)
```

**Called-by <-**
- [[FfDecoder.Decode]]
- [[FfEncoder.Encode]]
- [[StreamingEncoderSink.Initialize]]
- [[FfUtils.SelectChannelLayout]]

