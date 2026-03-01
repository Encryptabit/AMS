---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# FfUtils::FormatFraction
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`


#### [[FfUtils.FormatFraction]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string FormatFraction(double value, double min = 0, double max = 1)
```

**Calls ->**
- [[FfUtils.FormatNumber]]

**Called-by <-**
- [[FfFilterGraph.FormatFraction]]

