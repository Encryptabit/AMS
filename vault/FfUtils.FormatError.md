---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs"
access_modifier: "public"
complexity: 2
fan_in: 4
fan_out: 0
tags:
  - method
---
# FfUtils::FormatError
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`


#### [[FfUtils.FormatError]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string FormatError(int errorCode)
```

**Called-by <-**
- [[FilterGraphExecutor.ConfigureChannelLayouts]]
- [[FilterGraphExecutor.ConfigureGraph]]
- [[FilterGraphExecutor.ConfigureIntOption]]
- [[FfUtils.ThrowIfError]]

