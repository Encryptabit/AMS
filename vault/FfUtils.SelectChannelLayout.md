---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 1
tags:
  - method
---
# FfUtils::SelectChannelLayout
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`


#### [[FfUtils.SelectChannelLayout]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AVChannelLayout SelectChannelLayout(AVCodecContext* ctx)
```

**Calls ->**
- [[FfUtils.CreateDefaultChannelLayout]]

