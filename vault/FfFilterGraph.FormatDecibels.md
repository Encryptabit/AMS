---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 1
tags:
  - method
---
# FfFilterGraph::FormatDecibels
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.FormatDecibels]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatDecibels(double value)
```

**Calls ->**
- [[FfUtils.FormatDecibels]]

**Called-by <-**
- [[FfFilterGraph.ACompressor]]
- [[FfFilterGraph.ALimiter]]

