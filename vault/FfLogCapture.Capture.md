---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfLogCapture.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# FfLogCapture::Capture
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfLogCapture.cs`


#### [[FfLogCapture.Capture]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static List<string> Capture(Action action)
```

**Calls ->**
- [[FfSession.EnsureFiltersAvailable]]

**Called-by <-**
- [[FfFilterGraph.CaptureLogs]]

