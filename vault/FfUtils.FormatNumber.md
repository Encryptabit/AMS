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
# FfUtils::FormatNumber
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`


#### [[FfUtils.FormatNumber]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string FormatNumber(double value, string format = "0.####")
```

**Called-by <-**
- [[FfFilterGraph.FormatDouble]]
- [[FfUtils.FormatDecibels]]
- [[FfUtils.FormatFraction]]
- [[FfUtils.FormatMilliseconds]]

