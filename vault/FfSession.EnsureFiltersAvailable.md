---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 2
tags:
  - method
---
# FfSession::EnsureFiltersAvailable
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfSession.cs`


#### [[FfSession.EnsureFiltersAvailable]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void EnsureFiltersAvailable()
```

**Calls ->**
- [[FfSession.EnsureFilterProbe]]
- [[FfSession.EnsureInitialized]]

**Called-by <-**
- [[FilterGraphExecutor..ctor]]
- [[FfLogCapture.Capture]]

