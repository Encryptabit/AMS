---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs"
access_modifier: "public"
complexity: 5
fan_in: 0
fan_out: 0
tags:
  - method
---
# FfUtils::CleanupThrowIfError
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`


#### [[FfUtils.CleanupThrowIfError]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void CleanupThrowIfError(string message, AVFormatContext* fmt, AVCodecContext* cc, AVIOContext* avio, GCHandle handle)
```

